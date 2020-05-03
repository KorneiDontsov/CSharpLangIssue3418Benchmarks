namespace CSharpLangIssue3418Benchmarks {
	using BenchmarkDotNet.Attributes;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;

	interface IEventLogger {
		ValueTask LogAsync
			(String eventName,
			 IEnumerable<(String name, Object? value)> args,
			 DateTimeOffset timestamp,
			 CancellationToken cancellationToken = default,
			 Int32? timeout = null);
	}

	class EventLoggerStub: IEventLogger {
		public ValueTask LogAsync
			(String eventName,
			 IEnumerable<(String name, Object? value)> args,
			 DateTimeOffset timestamp,
			 CancellationToken cancellationToken = default,
			 Int32? timeout = null) =>
			default;
	}

	class EventLogBuilderAsClass {
		readonly IEventLogger eventLogger;

		readonly String eventName;

		List<(String name, Object? value)>? args;

		DateTimeOffset? timestamp;

		CancellationToken cancellationToken;

		Int32? timeout;

		public EventLogBuilderAsClass (IEventLogger eventLogger, String eventName) {
			this.eventLogger = eventLogger;
			this.eventName = eventName;
		}

		public EventLogBuilderAsClass With (String name, Object? value) {
			args ??= new List<(String, Object?)>(capacity: 4);
			args.Add((name, value));
			return this;
		}

		public EventLogBuilderAsClass WithTimestamp (DateTimeOffset timestamp) {
			this.timestamp = timestamp;
			return this;
		}

		/// <remarks>
		///     These parameters could be set in <see cref = "Async" /> but there are examples where builder
		///     has several finalizing methods like 'Sync', 'AlsoReturnSomething', etc. Specifying there
		///     parameters in separate method or in constructor is more flexible.
		/// </remarks>
		public EventLogBuilderAsClass With (CancellationToken cancellationToken = default, Int32? timeout = null) {
			this.cancellationToken = cancellationToken;
			this.timeout = timeout;
			return this;
		}

		public ValueTask Async () =>
			eventLogger.LogAsync(
				eventName,
				args ?? Enumerable.Empty<(String, Object?)>(),
				timestamp ?? DateTimeOffset.Now,
				cancellationToken,
				timeout);
	}

	struct EventLogBuilderAsCopiedStruct {
		readonly IEventLogger eventLogger;

		readonly String eventName;

		List<(String name, Object? value)>? args;

		DateTimeOffset? timestamp;

		CancellationToken cancellationToken;

		Int32? timeout;

		public EventLogBuilderAsCopiedStruct (IEventLogger eventLogger, String eventName) {
			this.eventLogger = eventLogger;
			this.eventName = eventName;
			args = null;
			timestamp = null;
			cancellationToken = default;
			timeout = null;
		}

		public EventLogBuilderAsCopiedStruct With (String name, Object? value) {
			args ??= new List<(String, Object?)>(capacity: 4);
			args.Add((name, value));
			return this;
		}

		public EventLogBuilderAsCopiedStruct WithTimestamp (DateTimeOffset timestamp) {
			this.timestamp = timestamp;
			return this;
		}

		public EventLogBuilderAsCopiedStruct With
			(CancellationToken cancellationToken = default, Int32? timeout = null) {
			this.cancellationToken = cancellationToken;
			this.timeout = timeout;
			return this;
		}

		public ValueTask Async () =>
			eventLogger.LogAsync(
				eventName,
				args ?? Enumerable.Empty<(String, Object?)>(),
				timestamp ?? DateTimeOffset.Now,
				cancellationToken,
				timeout);
	}

	struct EventLogBuilderAsReferencedStruct {
		public readonly IEventLogger eventLogger;

		public readonly String eventName;

		public List<(String name, Object? value)>? args;

		public DateTimeOffset? timestamp;

		public CancellationToken cancellationToken;

		public Int32? timeout;

		public EventLogBuilderAsReferencedStruct (IEventLogger eventLogger, String eventName) {
			this.eventLogger = eventLogger;
			this.eventName = eventName;
			args = null;
			timestamp = null;
			cancellationToken = default;
			timeout = null;
		}
	}

	static class EventLogBuildFunctions {
		public static EventLogBuilderAsClass Log_Class (this IEventLogger eventLogger, String eventName) =>
			new EventLogBuilderAsClass(eventLogger, eventName);

		public static EventLogBuilderAsCopiedStruct Log_CopiedStruct
			(this IEventLogger eventLogger, String eventName) =>
			new EventLogBuilderAsCopiedStruct(eventLogger, eventName);

		public static EventLogBuilderAsReferencedStruct Log_ReferencedStruct
			(this IEventLogger eventLogger, String eventName) =>
			new EventLogBuilderAsReferencedStruct(eventLogger, eventName);

		public static ref EventLogBuilderAsReferencedStruct With
			(this ref EventLogBuilderAsReferencedStruct builder, String name, Object? value) {
			builder.args ??= new List<(String, Object?)>(capacity: 4);
			builder.args.Add((name, value));
			return ref builder;
		}

		public static ref EventLogBuilderAsReferencedStruct WithTimestamp
			(this ref EventLogBuilderAsReferencedStruct builder, DateTimeOffset timestamp) {
			builder.timestamp = timestamp;
			return ref builder;
		}

		public static ref EventLogBuilderAsReferencedStruct With
			(this ref EventLogBuilderAsReferencedStruct builder,
			 CancellationToken cancellationToken = default,
			 Int32? timeout = null) {
			builder.cancellationToken = cancellationToken;
			builder.timeout = timeout;
			return ref builder;
		}

		// TODO: If I use 'in' instead of 'ref', will this affect on performance?
		public static ValueTask Async (this ref EventLogBuilderAsReferencedStruct builder) =>
			builder.eventLogger.LogAsync(
				builder.eventName,
				builder.args ?? Enumerable.Empty<(String, Object?)>(),
				builder.timestamp ?? DateTimeOffset.Now,
				builder.cancellationToken,
				builder.timeout);
	}

	[SimpleJob, MemoryDiagnoser]
	public class BuilderAsObjectVsCopiedStructVsReferencedStruct {
		IEventLogger eventLogger = null!;

		[GlobalSetup]
		public void Setup () =>
			eventLogger = new EventLoggerStub();

		[Benchmark]
		public ValueTask Object () =>
			eventLogger.Log_Class("benchmark")
				.WithTimestamp(new DateTimeOffset(2020, 5, 3, 8, 17, 0, TimeSpan.Zero))
				.With("param0", "param0value")
				.With("param1", "param1value")
				.With("param2", "param2value")
				.With("param3", "param3value")
				.With("param4", "param4value")
				.With("param5", "param5value")
				.With("param6", "param6value")
				.With("param7", "param7value")
				.With("param8", "param8value")
				.With("param9", "param9value")
				.With("param10", "param10value")
				.With(CancellationToken.None, timeout: 15)
				.Async();

		[Benchmark]
		public ValueTask CopyingStruct () =>
			eventLogger.Log_CopiedStruct("benchmark")
				.WithTimestamp(new DateTimeOffset(2020, 5, 3, 8, 17, 0, TimeSpan.Zero))
				.With("param0", "param0value")
				.With("param1", "param1value")
				.With("param2", "param2value")
				.With("param3", "param3value")
				.With("param4", "param4value")
				.With("param5", "param5value")
				.With("param6", "param6value")
				.With("param7", "param7value")
				.With("param8", "param8value")
				.With("param9", "param9value")
				.With("param10", "param10value")
				.With(CancellationToken.None, timeout: 15)
				.Async();

		[Benchmark]
		public ValueTask ReferencedStruct () {
			var builder = eventLogger.Log_ReferencedStruct("benchmark");
			return
				builder.WithTimestamp(new DateTimeOffset(2020, 5, 3, 8, 17, 0, TimeSpan.Zero))
					.With("param0", "param0value")
					.With("param1", "param1value")
					.With("param2", "param2value")
					.With("param3", "param3value")
					.With("param4", "param4value")
					.With("param5", "param5value")
					.With("param6", "param6value")
					.With("param7", "param7value")
					.With("param8", "param8value")
					.With("param9", "param9value")
					.With("param10", "param10value")
					.With(CancellationToken.None, timeout: 15)
					.Async();
		}
	}
}
