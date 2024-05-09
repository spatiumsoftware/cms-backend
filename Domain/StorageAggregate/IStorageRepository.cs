
using Domain.Base;

namespace Domain.StorageAggregate
{
    public interface IStorageRepository
    {
        #region Storage
        Task<Storage> GetStorageByBlogId(int blogId);
        Task AddStorage (Storage storage);
        Task DeleteStorageForUserAsync(string userId);
        #endregion

        #region Folder
        Task<IEnumerable<Folder>> GetAllFoldersAsync();
        Task<Folder> GetFolderAsync(int folderId, int blogId);
        Task<Folder> GetFolderByBlogIdAndStorageId(int StorageId, int BlogId);
        Task<IEnumerable<Folder>> GetAllFoldersbystorageandBlogAsync(int StorageId, int BlogId);
        Task<Folder> GetFolderAndFileByStorageIdAndFolderId(int storageId, int folderId,int blogId);
        Task CreateFolderAsync(Folder folder);
        Task DeleteFolderAsync(int id, int blogId);
        void UpdateFolder(Folder folder);
        Task<bool> ChechNameExists(int blogId, int? parenetId , string FolderName);
        Task<Folder> GetFolderByName(string FolderName , int blogId , int? ParentId);
        Task DleteFolderbyusingBlogIdAndStorageId(int blogId, int storageId);
        #endregion

        #region Files
        Task<List<StaticFile>> GetAllFilesAsync(GetEntitiyParams fileParams, int blogId);
        Task<StaticFile> GetFileAsync(int id,int blogId);
        Task<List<StaticFile>> GetStaticFilesbyFolderIdStorageIdAsync(int FolderId, int blodId);
        Task CreateFileAsync(StaticFile File);
        Task DeleteFileAsync(int FileId, int blogId);
        void UpdateFile(StaticFile File);
        Task<Folder> GetFilesToExtract(int bloId,int? folderId);

        Task<bool> ChechFileNameExists(string FileName, int? folderid);
        Task<bool> CheckFileName(string FileName, int fileId, int? FolderId);
        Task<IEnumerable<StaticFile>> getFileByFolderId(int? FolderId);

        public double GetBlogFilesCapcity(int blogId);

         Task<List<StaticFile>> GetAllFiles(int blogId);
        #endregion
    }
}
