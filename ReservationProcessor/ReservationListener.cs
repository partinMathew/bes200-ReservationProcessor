using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMqUtils;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReservationProcessor
{
    public class ReservationListener : RabbitListener
    {
        private readonly ILogger<ReservationListener> Logger;
        private readonly ReservationHttpService Service;

        public ReservationListener(ILogger<ReservationListener> logger, IOptionsMonitor<RabbitOptions> options, ReservationHttpService service) : base(options)
        {
            Logger = logger;
            Service = service;
            base.QueueName = "reservations";
            base.ExchangeName = "";
        }

        public async override Task<bool> Process(string message)
        {
            // 1. Deserialize the message (above) into an object (so we'll need a model)
            var request = JsonSerializer.Deserialize<Reservation>(message);
            // 2. Maybe we'll log it out for fun.
            Logger.LogInformation($"Got a reservation. {Environment.NewLine} \t {request.For}");
            // 3. Do our "business stuff" (reservations with <= 3 books get approved, otherwise cancelled.
            
            // 4. Do the appropriate HTTP call to our API
            if(request.Books.Count <= 3)
            {
                // 5. If that post works, then return true (remove it from the Queue) 
                return await Service.MarkReservationAccecpted(request);
            }
            else
            {
                // 6. If it fails, it stays on the Queue (return false)
                return await Service.MarkReservationCancelled(request);
            }
        }
    }
}
