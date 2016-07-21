open System

type Heap(capacity:int) = 
  let items : int array = Array.zeroCreate capacity
  let mutable count : int = 0
  
  let less i j = items.[i] < items.[j]

  let swap i j =
    let temp = items.[i]
    items.[i] <- items.[j]
    items.[j] <- temp

  let rec swim k = 
    if k >= 1 then
      let parent = (k-1)/2
      match less parent k with
      | true -> 
          swap parent k
          swim parent
      | _ -> ()

  let rec sink k = 
    match 2*k + 1 < count, 2*k + 1 with
    | true, j -> 
        if j + 1 < count && less k (j+1) && less j (j+1) then
          swap k (j+1)
          sink (j+1)
        elif less k j then
          swap k j 
          sink j
    | false, _ -> ()

  member this.Push(item) = 
    items.[count] <- item
    count <- count + 1
    swim (count-1)

  member this.Pop() = 
    let result = items.[0]
    count <- count - 1
    items.[0] <- items.[count]
    items.[count] <- 0
    sink 0
    result

  member this.Inspect() = 
    printfn "%A" items

let heap = new Heap(64)
heap.Push(2)
heap.Push(3)
heap.Push(4)
heap.Push(5)
heap.Pop() |> printfn "%d" 
heap.Pop() |> printfn "%d" 
heap.Pop() |> printfn "%d" 
heap.Pop() |> printfn "%d" 
