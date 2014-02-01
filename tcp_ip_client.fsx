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

let tcp_ip_client (targetip, targetport) =
    let client = new TcpClient(targetip, targetport)
    mirror System.Console.In <| writer client |> Async.RunSynchronously
    client.Close()

tcp_ip_client (fsi.CommandLineArgs.[1], int fsi.CommandLineArgs.[2])
