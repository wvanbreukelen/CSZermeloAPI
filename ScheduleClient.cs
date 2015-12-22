using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ZermeloAPI {
    public class ScheduleClient {
		static private readonly DateTime EpochDate = new DateTime(1970, 1, 1);

		public ScheduleClient() { }

		public ScheduleClient(int user, string school, string code, bool generateToken) {
			User = user;
			School = school;
			Code = code;

			if (generateToken) {
				NewToken();
			}
		}

		public ScheduleClient(int user, string school, string token) {
			User = user;
			School = school;
			Token = token;
		}

		public string Token;
		public int User;
		public string School;

		private string _code;
		public string Code {
			set {
				_code = value.Replace(" ", "");
			}

			get {
				return _code;
			}
		}

		public void NewToken() {
			string url = new StringBuilder("https://")
				.Append(School)
				.Append(".zportal.nl/api/v2/oauth/token")
				.ToString();

			List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("grant_type", "authorization_code"),
				new KeyValuePair<string, string>("code", Code)
			};
			
			HttpClient client = new HttpClient();
			Task<HttpResponseMessage> response = client.PostAsync(url, new FormUrlEncodedContent(values));
			response.Wait();

			HttpResponseMessage result = response.Result;
			Task<string> responseString = result.Content.ReadAsStringAsync();
			responseString.Wait();

			try {
				JObject jsonObject = JObject.Parse(responseString.Result);
				Token = (string) jsonObject.GetValue("access_token");
			} catch (JsonReaderException) {
				throw new ScheduleException("Invalid authorization code.", ScheduleErrorType.InvalidCode);
			}
		}

		private Schedule GetSchedule(long begin, long end) {
			string url = new StringBuilder("https://")
				.Append(School)
				.Append(".zportal.nl/api/v2/appointments?user=")
				.Append(User).Append("&start=")
				.Append(begin).Append("&end=")
				.Append(end)
				.Append("&access_token=")
				.Append(Token)
				.ToString();

			HttpClient client = new HttpClient();
			Task<HttpResponseMessage> response = client.GetAsync(url);
			response.Wait();

			HttpResponseMessage result = response.Result;
			Task<string> responseString = result.Content.ReadAsStringAsync();
			responseString.Wait();

			JObject jsonObject;

            try {
				jsonObject = JObject.Parse(responseString.Result);
				Token = (string) jsonObject.GetValue("access_token");
			} catch (JsonReaderException) {
				throw new ScheduleException("Error fetching appointments", ScheduleErrorType.InvalidAppointmentJSON);
			}

			return new Schedule(jsonObject);
		}

		public Schedule GetSchedule(DateTime begin, DateTime end) {
            return GetSchedule(
				(long) (begin - EpochDate).TotalSeconds,
				(long) (end - EpochDate).TotalSeconds + 86400
			);
		}

		public Schedule GetSchedule(DateTime dateTime) {
			long totalSeconds = (long) (dateTime - EpochDate).TotalSeconds;
			return GetSchedule(totalSeconds, totalSeconds + 86400);
		}
	}
}
