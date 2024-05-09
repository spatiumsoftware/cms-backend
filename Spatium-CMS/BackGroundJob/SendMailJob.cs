using Domain.Interfaces;
using Domian.Interfaces;
using Infrastructure.Services.MailSettinService;
using Infrastructure.UOW;
using System.Security.Cryptography.Xml;

namespace Spatium_CMS.BackGroundJob
{
    public interface IsendMailScopedJob
    {
        Task SendMail(CancellationToken cancellationToken);

    }
    public class SendMailJob : IsendMailScopedJob
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ISendMailService sendMailService;
       
        public SendMailJob(IUnitOfWork unitOfWork , ISendMailService sendMailService)
        {
            this.unitOfWork = unitOfWork;
            this.sendMailService = sendMailService;
        }
        public async Task SendMail(CancellationToken cancellationToken)
        {
            var blogs = await unitOfWork.BlogRepository.GetAllBlogsAsync();
            foreach (var blog in blogs)
            {
                var defaultSubscription = await unitOfWork.SubscriptionRepository.GetDefaultSubscriptionAsync();
                if (blog.SubscriptionId != defaultSubscription.Id)
                {
                    var BillHistory = await unitOfWork.SubscriptionRepository.GetAllBillingHistoryAsync(blog.Id);

                    var currentData = DateTime.UtcNow.Date;
                    var lastbayment = BillHistory.LastOrDefault();
                    if (lastbayment != null)
                    {
                        var lastPaymentDateExpireData = lastbayment.CreationDate.Date.AddMonths(1).Date;
                        var DataToSendMessage = lastPaymentDateExpireData.AddDays(2);
                        bool x = currentData > DataToSendMessage;
                        if (currentData > DataToSendMessage)
                        {
                            // send email 
                            await sendMailService.SendMail(lastbayment.Email, "Spatium CMS ", $"your account is disabled, please renew your subscription to be able to use the system again");
                        }
                    }
                }
            }
        }
    }

    public class ConsumeScopedServiceHostedService : BackgroundService
    {
        public IServiceProvider Services { get; }

        public ConsumeScopedServiceHostedService(IServiceProvider services)
        {
            Services = services;
           
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation(
            //    "Consume Scoped Service Hosted Service running.");
            using PeriodicTimer p = new PeriodicTimer(TimeSpan.FromMinutes(1));
            while(await p.WaitForNextTickAsync(stoppingToken))
            {
                Console.WriteLine("called");
                await DoWork(stoppingToken);
            }
            
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            //_logger.LogInformation(
            //    "Consume Scoped Service Hosted Service is working.");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IsendMailScopedJob>();

                await scopedProcessingService.SendMail(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation(
            //    "Consume Scoped Service Hosted Service is stopping.");
             
            await base.StopAsync(stoppingToken);
        }
    }
}
