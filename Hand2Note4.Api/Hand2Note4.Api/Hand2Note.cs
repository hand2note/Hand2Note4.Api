using System.Diagnostics;
using System.IO.Pipes;

namespace Hand2Note4.Api;

public static class 
Hand2Note {
    
    public static void 
    Send(DynamicMessage message) => message.BinarySerializeToBytes().SendToHand2Note();
    
    private static void 
    SendToHand2Note(this byte[] bytes) {
        using var client = new NamedPipeClientStream(".", "hand2Note4Hud", PipeDirection.Out);
        client.Connect(timeout: 10000);
        client.Write(bytes, 0, bytes.Length);
    }

    public static bool
    IsHand2NoteRunning() => Process.GetProcessesByName("Hand2Note").Any();
}