using System.Text;

namespace HyperVCenter.Web.WebSockets;

/// <summary>
/// Encodes and decodes instructions in the Guacamole protocol format.
/// Format: length.value,length.value,...;
/// Example: "6.select,3.rdp;" → instruction("select", "rdp")
/// </summary>
public static class GuacamoleProtocol
{
    /// <summary>
    /// Encodes a Guacamole protocol instruction from an opcode and arguments.
    /// </summary>
    public static string EncodeInstruction(string opcode, params string[] args)
    {
        var sb = new StringBuilder();
        sb.Append($"{opcode.Length}.{opcode}");
        foreach (var arg in args)
        {
            var value = arg ?? string.Empty;
            sb.Append($",{value.Length}.{value}");
        }
        sb.Append(';');
        return sb.ToString();
    }

    /// <summary>
    /// Reads a single complete Guacamole instruction from a stream.
    /// Returns the raw instruction string including the terminating semicolon.
    /// </summary>
    public static async Task<string?> ReadInstructionAsync(
        Stream stream, CancellationToken ct, int maxLength = 65536)
    {
        var sb = new StringBuilder();
        var buffer = new byte[1];

        while (sb.Length < maxLength)
        {
            var read = await stream.ReadAsync(buffer, ct);
            if (read == 0) return null; // Stream closed

            var c = (char)buffer[0];
            sb.Append(c);

            if (c == ';')
                return sb.ToString();
        }

        throw new InvalidOperationException("Guacamole instruction exceeded max length.");
    }

    /// <summary>
    /// Parses a Guacamole instruction string into its opcode and arguments.
    /// </summary>
    public static (string opcode, string[] args) ParseInstruction(string instruction)
    {
        var elements = new List<string>();
        var pos = 0;

        while (pos < instruction.Length && instruction[pos] != ';')
        {
            // Read length
            var dotIndex = instruction.IndexOf('.', pos);
            if (dotIndex < 0) break;

            var lengthStr = instruction[pos..dotIndex];
            if (!int.TryParse(lengthStr, out var length))
                throw new FormatException($"Invalid Guacamole element length: '{lengthStr}'");

            // Read value
            var valueStart = dotIndex + 1;
            var value = instruction.Substring(valueStart, length);
            elements.Add(value);

            pos = valueStart + length;

            // Skip separator (, or ;)
            if (pos < instruction.Length && (instruction[pos] == ',' || instruction[pos] == ';'))
                pos++;
        }

        if (elements.Count == 0)
            throw new FormatException("Empty Guacamole instruction.");

        return (elements[0], elements.Skip(1).ToArray());
    }
}
