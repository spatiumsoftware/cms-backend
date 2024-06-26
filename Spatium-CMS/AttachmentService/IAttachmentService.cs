﻿using AutoMapper;
using Domain.StorageAggregate;

namespace Spatium_CMS.AttachmentService
{
    public interface IAttachmentService
    {
        Task<string> SaveAttachment(string dirctoryDestination, IFormFile formfile, string source, string imageName);
        string Resolve(StaticFile source, FileResponse destination, string destMember, ResolutionContext context);
        string GetDesireFileName(IFormFile file, string desiredFileName);
        void ValidateFileSize(IFormFile file);
        void CheckFileExtension(IFormFile file);
        string GetFileExtention(IFormFile file);

        #region Extarct Files
        Task CreateZipArchive(List<string> filesToZip, string zipArchivePath);
        List<string>FilesToExtract(Folder folder);
        List<string> RootFilesToExtarct(IEnumerable<StaticFile> Files);
        #endregion


    }
}
