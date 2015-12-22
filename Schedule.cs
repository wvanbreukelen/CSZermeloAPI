using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZermeloAPI {
	public class Schedule {
		public readonly List<Day> Days;

		public int Count {
			get {
				return Days.Count;
			}
		}

		public Day Get(int index) {
			return Days[index];
		}

		public IEnumerator GetEnumerator() {
			return Days.GetEnumerator();
		}

		public Schedule(JObject json) {
			JObject response = (JObject) json.GetValue("response");

			if ((int) response.GetValue("status") != 200) {
				throw new ScheduleException((string) response.GetValue("details"), ScheduleErrorType.FailedFetchingSchedule);
			}

			Days = GetDays((JArray) response.GetValue("data"));
		}

		private List<Day> GetDays(JArray appointments) {
			List<Day> days = new List<Day> { };

			foreach (JToken jAppointment in appointments) {
				Appointment appointment = (Appointment) JsonConvert.DeserializeObject(jAppointment.ToString(), typeof(Appointment));
				DateTime dateTime = new DateTime(appointment.Start * 10000000 + new DateTime(1970, 1, 1).Ticks);
				Day currentDay = null;

				foreach (Day day in days) {
					if (day.Date == dateTime.Date) {
						currentDay = day;
						break;
					}
				}

				if (currentDay == null) {
					currentDay = new Day(dateTime.Date);
					days.Add(currentDay);
				}

				currentDay.Add(appointment);
				currentDay.Sort();
			}

			days.Sort();

			return days;
		}
	}

	public class Day : IComparable<Day> {
		public readonly DateTime Date;
		private List<Appointment> Appointments = new List<Appointment> { };

		public Day(DateTime date) {
			Date = date;
		}

		public IEnumerator GetEnumerator() {
			return Appointments.GetEnumerator();
		}

		public void Add(Appointment appointment) {
			Appointments.Add(appointment);
		}

		public Appointment Get(int index) {
			return Appointments[index];
		}

		public void Sort() {
			Appointments.Sort();
		}

		public int CompareTo(Day other) {
			return (int) (Date - other.Date).TotalDays;
		}
	}

	public class Appointment : IComparable<Appointment> {
		public int Id;
		public long Start;
		public long End;
		public string[] Subjects;
		public string[] Teachers;
		public string[] Groups;
		public string[] Locations;
		public string Type;
		public string Remark;
		public bool Valid;
		public bool Cancelled;
		public bool Modified;
		public bool Moved;
		public string ChangeDescription;
		public int StartTimeSlot;
		public int EndTimeSlot;
		public string Branch;
		public string BranchOfSchool;
		public long Created;
		public long LastModified;
		public bool Hidden;
		public int AppointmentInstance;
		public bool New;

		public int CompareTo(Appointment other) {
			return (int) (Start - other.Start);
		}
	}
}
