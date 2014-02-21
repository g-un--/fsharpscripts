open System.IO
open System.Threading
open System.Net
open System.Net.Sockets


let rec mirror (reader:TextReader) (writer:StreamWriter) = async {
    let! line = reader.ReadLineAsync() |> Async.AwaitTask
    match line with
    | null -> ()
    | _ ->
        let task = writer.WriteLineAsync(line)
        do! task.ContinueWith(fun t -> ()) |> Async.AwaitTask
        return! mirror reader writer
}

let writer (client:TcpClient) =
    let newWriter = new StreamWriter(client.GetStream())
    newWriter.AutoFlush <- true
    newWriter

let tcp_ip_server (sourceip,sourceport) =
    let server = new TcpListener(IPAddress.Parse(sourceip),sourceport)
    server.Start()
    while true do
        let client = server.AcceptTcpClient()
        let t = new Thread(ThreadStart(fun _ ->
            try
                mirror System.Console.In <| writer client |> Async.RunSynchronously 
            finally 
                client.Close())
        , IsBackground = true)
        t.Start()

tcp_ip_server (fsi.CommandLineArgs.[1], int fsi.CommandLineArgs.[2])
