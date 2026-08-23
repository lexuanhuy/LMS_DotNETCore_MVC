namespace LMS_DotNETCore_MVC.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName);
        void DeleteFile(string relativePath);
    }
}
