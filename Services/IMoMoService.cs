using LMS_DotNETCore_MVC.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LMS_DotNETCore_MVC.Services
{
    public interface IMoMoService
    {
        Task<string> CreatePaymentAsync(Order order, HttpContext httpContext);
        bool VerifySignature(string signature, string rawHash);
    }
}
