using MedLink.Application.Common.Sms;
using MedLink.Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MedLink.Application.Services
{
    public class SmsService : ISmsService
    {
        private readonly TwilioSettings _twilioSettings;

        public SmsService(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
        }

        public async Task<MessageResource> SendSmsAsync(string to, string message)
        {
            TwilioClient.Init(_twilioSettings.AccountSID, _twilioSettings.AuthToken);

            var result = await MessageResource.CreateAsync(
                to: new PhoneNumber(to),
                from: new PhoneNumber(_twilioSettings.PhoneNumber),
                body: message
            );

            return result;
        }

        public async Task<string> SendVerificationTokenAsync(string phoneNumber)
        {
            TwilioClient.Init(_twilioSettings.AccountSID, _twilioSettings.AuthToken);

            var verification = await Twilio.Rest.Verify.V2.Service.VerificationResource.CreateAsync(
                to: phoneNumber,
                channel: "sms",
                pathServiceSid: _twilioSettings.VerifyServiceSid
            );

            return verification.Status;
        }

        public async Task<bool> VerifyTokenAsync(string phoneNumber, string token)
        {
            TwilioClient.Init(_twilioSettings.AccountSID, _twilioSettings.AuthToken);

            var verificationCheck = await Twilio.Rest.Verify.V2.Service.VerificationCheckResource.CreateAsync(
                to: phoneNumber,
                code: token,
                pathServiceSid: _twilioSettings.VerifyServiceSid
            );

            return verificationCheck.Status == "approved";
        }
    }
}
