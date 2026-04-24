using System.Buffers.Binary;

namespace WebServer.Infrastructure.Data;

internal static class PostgresEmployeeIdMapper
{
    public static Guid ToUserId(int employeeId)
    {
        Span<byte> bytes = stackalloc byte[16];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, employeeId);
        return new Guid(bytes);
    }

    public static bool TryGetEmployeeId(Guid userId, out int employeeId)
    {
        Span<byte> bytes = stackalloc byte[16];
        userId.TryWriteBytes(bytes);

        for (var index = sizeof(int); index < bytes.Length; index++)
        {
            if (bytes[index] != 0)
            {
                employeeId = 0;
                return false;
            }
        }

        employeeId = BinaryPrimitives.ReadInt32LittleEndian(bytes);
        return employeeId > 0;
    }
}
