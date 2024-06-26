﻿using AutoMapper;
using Domain.StorageAggregate;
using Org.BouncyCastle.Asn1.X509;
using System.IO.Compression;
using Utilities.Exceptions;

namespace Spatium_CMS.AttachmentService
{
    public class AttachmmentService : IAttachmentService
    {
        private readonly IConfiguration configration;
        private readonly IWebHostEnvironment environment;

        public AttachmmentService()
        {

        }
        public AttachmmentService(IConfiguration configration,IWebHostEnvironment environment)
        {
            this.configration = configration;
            this.environment = environment;
        }
        public async Task<string> SaveAttachment(string dirctoryDestination, IFormFile formfile, string source,
            string imageName)
        {

            string filePath = source + "\\" + dirctoryDestination;
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string fileName = $"{imageName}{Path.GetExtension(formfile.FileName)}";
            string ImagePath = Path.Combine(filePath, fileName);

            if (File.Exists(ImagePath))
            {
                File.Delete(ImagePath);
            }

            using (FileStream stream = File.Create(ImagePath))
            {
                await formfile.CopyToAsync(stream);
            }
            string url = dirctoryDestination + "/" + fileName;
            return url;
        }


        public string Resolve(StaticFile source, FileResponse destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.UrlPath))
            {
                return $"{configration["ApiBaseUrl"]}/{source.UrlPath}";
            }
            else
                return string.Empty;
        }
        public string GetDesireFileName(IFormFile file, string desiredFileName)
        {

            var originalFileName = file.FileName;
            var fileExtension = Path.GetExtension(originalFileName);
            var desiredFileNameWithExtension = $"{desiredFileName}{fileExtension}";
            return desiredFileNameWithExtension;
        }

        public void ValidateFileSize(IFormFile file)
        {
            long fileSizeInBytes = file.Length;
            string fileType = GetFileTypeFromExtension(file.FileName);

            switch (fileType.ToLower())
            {
                case "image":
                    if (fileSizeInBytes < 5 * 1024 || fileSizeInBytes > 5 * 1024 * 1024) // 5KB - 5MB
                    {
                        throw new SpatiumException("Invalid file size for Image. Size should be between 5KB and 5MB");
                    }
                    break;
                case "video":
                    if (fileSizeInBytes < 1024 * 1024 || fileSizeInBytes > 500 * 1024 * 1024) // 1MB - 500MB
                    {
                        throw new SpatiumException("Invalid file size for Video. Size should be between 1MB and 500MB");
                    }
                    break;
                case "record":
                    if (fileSizeInBytes < 1024 * 1024 || fileSizeInBytes > 100 * 1024 * 1024) // 1MB - 100MB
                    {
                        throw new SpatiumException("Invalid file size for Record. Size should be between 1MB and 100MB");
                    }
                    break;
                case "file":
                    if (fileSizeInBytes < 0 || fileSizeInBytes > 150 * 1024 * 1024) // 0B - 150MB
                    {
                        throw new SpatiumException("Invalid file size for File. Size should be between 0B and 150MB.");
                    }
                    break;
                default:
                    throw new SpatiumException("Invalid file type.");
            }
        }
        public string GetFileTypeFromExtension(string fileName)
        {
            string extension = Path.GetExtension(fileName)?.TrimStart('.').ToLower();
            switch (extension)
            {
                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                case "bin":
                case "webp":
                    return "image";
                case "mp4":
                case "3g2":
                case "3gp":
                case "wmv":
                case "webm":
                case "m4v":
                    return "video";
                case "mp3":              
                case "wma":  
                return "record";
                case "csv":
                case "xlsx":
                case "xls":
                case "doc":
                case "docs":
                case "pdf":
                case "txt":
                case "xml":
                    return "file";
                default:
                    return "unknown";
            }
        }  
        public void CheckFileExtension(IFormFile file)
        {
            var originalFileName = file.FileName;
            var fileExtension = Path.GetExtension(originalFileName);           
            var cleanFileExtension = fileExtension.TrimStart('.').ToLower();            
            var extentions = new List<string> { "mp4", "3g2", "3gp", "wmv", "webm",
                 "mp3",  "wma", 
                "jpg", "png", "webp", "gif", "bin" ,
                "csv", "xlsx", "xls", "doc", "docs", "pdf", "txt", "xml"};
            var flag = extentions.Any(x => x.Equals(cleanFileExtension));
            if (!flag)
            {
                throw new SpatiumException("This Extension is Not Supported");
            }
        }

        public string GetFileExtention(IFormFile file)
        {
            var originalFileName = file.FileName;
            var fileExtension = Path.GetExtension(originalFileName);
            return fileExtension;
        }

        #region Extarct Files
        public List<string> FilesToExtract(Folder folder)
        {
            var filesToExtract = new List<string>();
            if (folder == null)
            {
                return filesToExtract;
            }
            foreach (var file in folder.Files)
            {
                filesToExtract.Add(Path.Combine(environment.WebRootPath, file.UrlPath));
            }
            if (folder.Folders != null)
            {
                foreach (var subfolder in folder.Folders)
                {
                    var subfolderFiles = FilesToExtract(subfolder);
                    filesToExtract.AddRange(subfolderFiles);
                }
            }
            return filesToExtract;
        }
        public List<string>RootFilesToExtarct(IEnumerable<StaticFile>Files)
        {
            var filesToExtract = new List<string>();
            foreach (var file in Files)
            {
                filesToExtract.Add(Path.Combine(environment.WebRootPath, file.UrlPath));
            }
            return filesToExtract;
        }
        public async Task CreateZipArchive(List<string> filesToZip, string zipArchivePath)
        {
            using (var zipArchive = ZipFile.Open(zipArchivePath, ZipArchiveMode.Create))
            {
                foreach (var filePath in filesToZip)
                {
                    var entryName = Path.GetRelativePath(environment.WebRootPath, filePath);
                    var entry = zipArchive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using (var fileStream = new FileStream(filePath, FileMode.Open))
                    using (var entryStream = entry.Open())
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }
        }
        #endregion
    }
}
