﻿using Domain.Base;
using Domain.StorageAggregate;
using Infrastructure.Database.Database;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

using Utilities.Exceptions;
namespace Infrastructure.Database.Repository
{
    public class StorageRepository : RepositoryBase, IStorageRepository
    {
        public StorageRepository(SpatiumDbContent SpatiumDbContent) : base(SpatiumDbContent)
        { }
        #region Storage
        public async Task<Storage> GetStorageByBlogId(int blogId)
        {
            return await SpatiumDbContent.Storages.Where(s => s.BlogId == blogId).Include(s => s.Folders).ThenInclude(f => f.Files).FirstOrDefaultAsync();
        }
        public async Task DeleteStorageForUserAsync(string userId)
        {
            var storages = await SpatiumDbContent.Storages.Where(s => s.ApplicationUserId == userId).ToListAsync();            
            SpatiumDbContent.Storages.RemoveRange(storages);
        }
        public async Task DleteFolderbyusingBlogIdAndStorageId(int blogId,int storageId)
        {
            var Folder=await SpatiumDbContent.Folders.Where(f=>f.BlogId==blogId&&f.StorageId==storageId).FirstOrDefaultAsync();
            if (Folder==null)
            {
                SpatiumDbContent.Folders.Remove(Folder); 
            }
        }



        #endregion

        #region Folder 

        public async Task CreateFolderAsync(Folder folder)
        {
            await SpatiumDbContent.Folders.AddAsync(folder);
        }

        public async Task DeleteFolderAsync(int folderId, int blogId)
        {
            var folder = await GetFolderAsync(folderId, blogId);
            if (folder != null)
            {
                SpatiumDbContent.Folders.Remove(folder);
            }
        }

        public async Task<IEnumerable<Folder>> GetAllFoldersAsync()
        {
            return await SpatiumDbContent.Folders.ToListAsync();
        }
        public async Task<IEnumerable<Folder>> GetAllFoldersbystorageandBlogAsync(int StorageId, int BlogId)
        {
            return await SpatiumDbContent.Folders.Where(f => f.BlogId == BlogId && StorageId == f.StorageId).ToListAsync();
        }
        public async Task<Folder> GetFolderAsync(int id, int blogId)
        {
            return await SpatiumDbContent.Folders.Where(x => x.Id == id && x.BlogId == blogId).FirstOrDefaultAsync();

        }
        public async Task<Folder> GetFolderAndFileByStorageIdAndFolderId(int storageId, int folderId, int blogId)
        {
            return await SpatiumDbContent.Folders.Include(f => f.Files).Include(f => f.Folders).FirstOrDefaultAsync(f => f.StorageId == storageId && f.Id == folderId && f.BlogId == blogId);

        }

        public void UpdateFolder(Folder folder)
        {
            SpatiumDbContent.Folders.Update(folder);
        }
        public async Task<bool> ChechNameExists(int blogId, int? parenetId, string FolderName)
        {
            bool flag;
            //if(parenetId == null)
            //   flag =await SpatiumDbContent.Folders.SingleOrDefaultAsync(f => f.BlogId == blogId && f.ParentId == null && f.Name.ToLower() == FolderName ) is not null? true : false;
            //else
            flag = await SpatiumDbContent.Folders.SingleOrDefaultAsync(f => f.BlogId == blogId && f.ParentId == parenetId && f.Name.ToLower() == FolderName) is not null ? true : false;
            return flag;
        }

        public async Task<Folder> GetFolderByName(string FolderName, int blogId, int? ParentId)
        {
            return await SpatiumDbContent.Folders.SingleOrDefaultAsync(f => f.Name == FolderName && f.BlogId == blogId && f.ParentId == ParentId);
        }
        #endregion

        #region File

        public async Task CreateFileAsync(StaticFile File)
        {
            await SpatiumDbContent.Files.AddAsync(File);
        }
        public async Task DeleteFileAsync(int FileId, int blogId)
        {
            var file = await GetFileAsync(FileId, blogId);
            if (file is not null)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.BlogId.ToString(), file.Name + file.Extention);

                if (File.Exists(uploadPath))
                {
                    File.Delete(uploadPath);
                }
                SpatiumDbContent.Files.Remove(file);
            }else
            {
                throw new SpatiumException($"File  NOT Exist!");
            }
        }
        public async Task<List<StaticFile>> GetAllFiles(int blogId)
        {
            return await SpatiumDbContent.Files.Where(f=>f.BlogId == blogId).ToListAsync();
        }
        public async Task<List<StaticFile>> GetAllFilesAsync(GetEntitiyParams fileParams, int blogId)
        {
            var query = SpatiumDbContent.Files.Where(f => f.BlogId == blogId).AsQueryable();

            
            if (!string.IsNullOrEmpty(fileParams.FilterColumn))
            {
                if (!string.IsNullOrEmpty(fileParams.FilterValue) && fileParams.StartDate == null && fileParams.EndDate == null)
                {
                    query = query.ApplyFilter(fileParams.FilterColumn, fileParams.FilterValue);
                }
                if (fileParams.StartDate != null && fileParams.EndDate != null && fileParams.FilterColumn.ToLower() == "creationdate")
                {
                    query = query.Where(p => p.CreationDate.Date >= fileParams.StartDate.Value.Date && p.CreationDate.Date <= fileParams.EndDate.Value.Date);
                }
                if (fileParams.StartDate != null && fileParams.EndDate != null && fileParams.FilterColumn.ToLower() == "lastupdate")
                {
                    query = query.Where(p => p.LastUpdate >= fileParams.StartDate && p.LastUpdate == fileParams.EndDate || p.LastUpdate < fileParams.EndDate);
                }

            }


            if (!string.IsNullOrEmpty(fileParams.SortColumn))
            {
                query = query.ApplySort(fileParams.SortColumn, fileParams.IsDescending);
            }

            if (!string.IsNullOrEmpty(fileParams.SearchColumn) && !string.IsNullOrEmpty(fileParams.SearchValue))
            {
                query = query.ApplySearch(fileParams.SearchColumn, fileParams.SearchValue);
            }

            var paginatedQuery = query.Skip((fileParams.Page - 1) * fileParams.PageSize).Take(fileParams.PageSize);
            return await paginatedQuery.ToListAsync();
        }

        public async Task<List<StaticFile>> GetStaticFilesbyFolderIdStorageIdAsync(int FolderId,int blodId) {
        
        return await SpatiumDbContent.Files.Where(f=>f.FolderId==FolderId&&f.BlogId==blodId).ToListAsync();
       
        }

        public async Task<StaticFile> GetFileAsync(int id, int blogId)
        {
            return await SpatiumDbContent.Files.Where(x => x.BlogId == blogId && x.Id == id).FirstOrDefaultAsync();
        }
        public void UpdateFile(StaticFile File)
        {
            SpatiumDbContent.Files.Update(File);
        }
        public async Task<Folder> GetFilesToExtract(int blogId, int? folderId)
        {
            return await SpatiumDbContent.Folders.Include(f => f.Files).Include(f => f.Folders).FirstOrDefaultAsync(f => f.BlogId == blogId && f.Id == folderId);
        }

        public async Task AddStorage(Storage storage)
        {
            await SpatiumDbContent.Storages.AddAsync(storage);
        }

        public async Task<bool> ChechFileNameExists(string FileName,  int? folderid)
        {
            var IsExist = await SpatiumDbContent.Files.FirstOrDefaultAsync(f => f.Name == FileName &&f.FolderId==folderid) is not null ? true : false;
            return IsExist;
        }
  
        public async Task<bool> CheckFileName(string FileName, int fileId, int? FolderId)
        {
            
            var IsExist = await SpatiumDbContent.Files
                .FirstOrDefaultAsync(f => f.FolderId == FolderId && f.Name == FileName && f.Id != fileId);
            return IsExist != null;
        }


        public async Task<IEnumerable<StaticFile>> getFileByFolderId(int? FolderId)
        {
            if (FolderId == null)
                return await SpatiumDbContent.Files.Where(f=>f.FolderId == null).ToListAsync();  
            else
                return await SpatiumDbContent.Files.Where(f => f.FolderId == FolderId).ToListAsync();
        }

        public async Task<Folder> GetFolderByBlogIdAndStorageId(int StorageId, int BlogId)
        {
            var Folder=await SpatiumDbContent.Folders.Where(f=>f.BlogId==BlogId&&StorageId==f.StorageId).FirstOrDefaultAsync();
            return Folder;
        }


        public double GetBlogFilesCapcity(int blogId)
        {
            var files =  SpatiumDbContent.Files.Where(f => f.BlogId == blogId);
            if (files==null)
            {
                return 0.0;
            }
            var capcity = 0.0;
            foreach (var f in files) { 
                capcity += double.Parse(f.FileSize);
            }
            return capcity/1024/1024;
        }
        #endregion
    }
}
