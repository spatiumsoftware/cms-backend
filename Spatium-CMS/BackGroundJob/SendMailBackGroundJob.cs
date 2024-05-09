//using Domain.Interfaces;
//using Domian.Interfaces;
//using Infrastructure.Services.AuthinticationService.Models;
//using MailKit;
//using Utilities.Enums;
//using Utilities.Results;

//namespace Spatium_CMS.BackGroundJob
//{
//    public class BackGroundSendMail : IHostedService, IDisposable
//    {

//        private Timer _timer;
//        private readonly IUnitOfWork unitOfWork;
//        private readonly ISendMailService sendMailService;

//        public BackGroundSendMail(IUnitOfWork unitOfWork , ISendMailService sendMailService)
//        {
//            this.unitOfWork = unitOfWork;
//            this.sendMailService = sendMailService;
//        }
//        public Task StartAsync(CancellationToken cancellationToken)
//        {
//            // run start 
//            _timer = new Timer(SendMail, state: null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
//            return Task.CompletedTask;
//        }

//        public Task StopAsync(CancellationToken cancellationToken)
//        {
//            // run end  
//            _timer?.Change(Timeout.Infinite, 0);
//            throw new NotImplementedException();
//        }
//        private async void SendMail(object? state)
//        {
//            var blogs = await unitOfWork.BlogRepository.GetAllBlogsAsync();
//            foreach (var blog in blogs)
//            {
//                var defaultSubscription = await unitOfWork.SubscriptionRepository.GetDefaultSubscriptionAsync();
//                if (blog.SubscriptionId != defaultSubscription.Id)
//                {
//                    var BillHistory = await unitOfWork.SubscriptionRepository.GetAllBillingHistoryAsync(blog.Id);

//                    var currentData = DateTime.UtcNow.Date;
//                    var lastbayment = BillHistory.LastOrDefault();
//                    if (lastbayment != null)
//                    {
//                        var lastPaymentDate = lastbayment.CreationDate.Date.AddMonths(1).Date;
//                        var after2day = lastPaymentDate.AddDays(2);
//                        bool x = currentData > lastPaymentDate.AddDays(2);
//                        if (currentData > lastPaymentDate.AddDays(2))
//                        {
//                          // send email 
//                           await sendMailService.SendMail(lastbayment.Email, "Spatium CMS ", $"your account is disabled, please renew your subscription to be able to use the system again");
//                        }
//                    }
//                }
//            }
//        }
//        public void Dispose()
//        {
//            _timer?.Dispose();
//        }

//    }
//}
