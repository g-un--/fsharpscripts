open System.IO
open System.Threading
open System.Net
open System.Net.Sockets

let read (clientStream:Stream) = 
    use streamReader = new StreamReader(clientStream)
    let rec readline reader = async {
        let! line = streamReader.ReadLineAsync() |> Async.AwaitTask
        match line with
        | null -> ()
        | _ -> 
            do printfn "%s" line; 
            return! readline reader
    }
    readline streamReader |> Async.RunSynchronously

let stream (client:TcpClient) = client.GetStream()

let tcp_ip_server (sourceip,sourceport) =
    let server = new TcpListener(IPAddress.Parse(sourceip),sourceport)
    server.Start()
    while true do
        let client = server.AcceptTcpClient()
        let t = new Thread(ThreadStart(fun _ ->
            try
                stream (client) |> read 
            with |_ -> client.Close())
        , IsBackground = true)
        t.Start()

tcp_ip_server (fsi.CommandLineArgs.[1], int fsi.CommandLineArgs.[2])
