using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.Text;

public class JsonArrayFileSink : ILogEventSink, IDisposable
{
    private readonly StreamWriter _writer;
    private readonly ITextFormatter _formatter;
    private bool _firstEvent = true;
    private readonly object _syncRoot = new object();

    public JsonArrayFileSink(string path, ITextFormatter formatter)
    {
        _formatter = formatter;
        // Open the file for writing (overwriting any existing file)
        _writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None), Encoding.UTF8);
        // Write the opening bracket of the JSON array
        _writer.WriteLine("[");
    }

    public void Emit(LogEvent logEvent)
    {
        lock (_syncRoot)
        {
            if (!_firstEvent)
            {
                _writer.WriteLine(",");
            }
            else
            {
                _firstEvent = false;
            }

            // Use a StringWriter with our pretty formatter to get the formatted JSON
            using (var sw = new StringWriter())
            {
                _formatter.Format(logEvent, sw);
                string json = sw.ToString().TrimEnd();
                _writer.Write(json);
            }
            _writer.Flush();
        }
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            // Write the closing bracket of the JSON array
            _writer.WriteLine();
            _writer.WriteLine("]");
            _writer.Flush();
            _writer.Dispose();
        }
    }
}

