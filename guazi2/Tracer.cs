using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace guazi2
{
    public class Tracer : IDisposable
    {

        private ReaderWriterLock _lock;
        private StreamWriter _writer;
        private string _default_trace_fmt = "[{0}] [{1}] [{2} - {3}] {4}";

        private void write_trace(string fmt, params object[] args)
        {
            _lock.AcquireWriterLock(Timeout.Infinite);
            _writer?.WriteLine(fmt, args);
            Debug.Print(fmt, args);
            _lock.ReleaseWriterLock();
        }
        public Tracer(string log_path = null)
        {
            _lock = new ReaderWriterLock();
            _writer = null;
            if (!string.IsNullOrEmpty(log_path))
            {
                try
                {
                    _writer = new StreamWriter(log_path, false);
                }
                catch (Exception ex)
                {
                    _writer = null;
                    write_trace(ex.ToString());
                }
            }
        }
        private string get_current_thread_id()
        {
            return Thread.CurrentThread.ManagedThreadId.ToString();
        }
        private string get_current_thread_name()
        {
            return Thread.CurrentThread.Name;
        }
        private string get_current_time()
        {
            return DateTime.Now.ToString("HH:mm:ss.fff");
        }
        public void TraceInfo(string info)
        {
            write_trace(_default_trace_fmt, get_current_time(), "Info", get_current_thread_id(), get_current_thread_name(), info);
        }
        public void TraceWarning(string info)
        {
            write_trace(_default_trace_fmt, get_current_time(), "Warning", get_current_thread_id(), get_current_thread_name(),  info);
        }
        public void TraceError(string info)
        {
            write_trace(_default_trace_fmt, get_current_time(), "Error", get_current_thread_id(), get_current_thread_name(), info);
        }

        public void Dispose()
        {
            ((IDisposable)_writer).Dispose();
        }
    }
}
