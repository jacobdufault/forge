using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Neon.Utility {
    public interface ILogProvider {
        void Info(string message, object context = null);
        void Warning(string message, object context = null);
        void Error(string message, object context = null);
        IDisposable Region(string message, object context = null);
    }

    class IndentDisposable : IDisposable {
        public IndentDisposable() {
            System.Diagnostics.Debug.Indent();
        }

        public void Dispose() {
            System.Diagnostics.Debug.Unindent();
        }
    }

    public class ConsoleLogProvider : ILogProvider {
        public IDisposable Region(string message, object context = null) {
            Info(message, context);
            return new IndentDisposable();
        }

        public void Info(string message, object context = null) {
            if (context == null) {
                System.Diagnostics.Debug.WriteLine(string.Format("[info] {0}", message));
                Console.Out.WriteLine("[info] {0}", message);
            }

            else {
                System.Diagnostics.Debug.WriteLine(string.Format("[info {0}] {1}", context, message));
                Console.Out.WriteLine("[info {0}] {1}", context, message);
            }
        }

        public void Warning(string message, object context = null) {
            if (context == null) {
                System.Diagnostics.Debug.WriteLine(string.Format("[warning] {0}", message));
                Console.Out.WriteLine("[warning] {0}", message);
            }

            else {
                System.Diagnostics.Debug.WriteLine(string.Format("[warning {0}] {1}", context, message));
                Console.Out.WriteLine("[warning {0}] {1}", context, message);
            }
        }

        public void Error(string message, object context = null) {
            if (context == null) {
                System.Diagnostics.Debug.WriteLine(string.Format("[error] {0}", message));
                Console.Error.WriteLine("[error] {0}", message);
            }

            else {
                System.Diagnostics.Debug.WriteLine(string.Format("[error {0}] {1}", context, message));
                Console.Error.WriteLine("[error {0}] {1}", context, message);
            }
        }
    }

    // for unity log provider, use Interlocked.Increment + an int

    public static class Log {
        public static ILogProvider _logProvider = new ConsoleLogProvider();

        public static void Region(string format, params object[] args) {
            _logProvider.Region(string.Format(format, args));
        }

        public static void Info(object message) {
            _logProvider.Info(message.ToString());
        }

        public static void Info(string format, params object[] args) {
            _logProvider.Info(string.Format(format, args));
        }

        public static void Warning(object message) {
            _logProvider.Warning(message.ToString());
        }

        public static void Warning(string format, params object[] args) {
            _logProvider.Warning(string.Format(format, args));
        }

        public static void Error(object message) {
            _logProvider.Error(message.ToString());
        }

        public static void Error(string format, params object[] args) {
            _logProvider.Error(string.Format(format, args));
        }
    }
}
