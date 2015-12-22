using System;

namespace ZermeloAPI {
	public enum ScheduleErrorType {
		Unknown,
		InvalidCode,
		InvalidAppointmentJSON,
		FailedFetchingSchedule
	}

	class ScheduleException : Exception {
		public readonly ScheduleErrorType ErrorType;

		public ScheduleException(string errorMessage, ScheduleErrorType errorType) : base(errorMessage) {
			ErrorType = errorType;
		}

		public ScheduleException(string errorMessage) : base(errorMessage) {
			ErrorType = ScheduleErrorType.Unknown;
		}
	}
}
