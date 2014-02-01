open System.IO
open System.Threading
open System.Net
open System.Net.Sockets

let mirror (clientStream:Stream) (serverStream:Stream) = async {
    while true do
        let! onebyte = clientStream.AsyncRead(1)
        do! serverStream.AsyncWrite(onebyte) 
}

let proxy (clientStream:Stream) (serverStream:Stream) = 
    [| mirror clientStream serverStream; mirror serverStream clientStream |]
        |> Async.Parallel
        |> Async.RunSynchronously 
        
let stream (client:TcpClient) = client.GetStream()

let tcp_ip_proxy (sourceip,sourceport) (targetip,targetport) = 
    let server = new TcpListener(IPAddress.Parse(sourceip),sourceport)
    server.Start()
    let rec handle listener = async {
        let! client = server.AcceptTcpClientAsync() |> Async.AwaitTask
        let t = new Thread(ThreadStart(fun _ -> 
            try
                let up = new TcpClient(targetip,targetport)
                stream (client) |> proxy <| stream(up) |> ignore
                up.Close()
            with 
            | _ -> client.Close())
        , IsBackground = true)
        t.Start()
        return! handle listener    
    }
    handle server |> Async.RunSynchronously |> ignore

tcp_ip_proxy 
    (fsi.CommandLineArgs.[1],int fsi.CommandLineArgs.[2]) 
    (fsi.CommandLineArgs.[3],int fsi.CommandLineArgs.[4])
