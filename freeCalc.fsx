type CalcApi<'r, 'next> =
| Add of 'r * ('r -> 'next)
| Multiply of 'r * ('r -> 'next)

type CalcProgram<'r> = 
| Result of 'r
| Next of CalcApi<'r, CalcProgram<'r>>

let mapApi f inst =
    match inst with
    | Add(item, next) -> Add(item, next >> f)
    | Multiply(item, next) -> Multiply(item, next >> f)

let returnProgram x = Result x

let rec bindProgram f program = 
    match program with 
    | Next inst -> Next (mapApi (bindProgram f) inst)
    | Result x -> f x

type CalcProgramBuilder() = 
    member this.Return(x) = returnProgram x
    member this.Bind(x,f) = bindProgram f x 

let calcProgram = CalcProgramBuilder()

let add x = Next(Add(x, Result))
let multiply x = Next(Multiply(x, Result))

let program = calcProgram { 
    let! sum = add 1 
    let! product = multiply 2
    return product
    }

let rec syncInterpreter state program = 
    match program with 
    | Result x -> x
    | Next(Add(x, next)) -> 
        let newState = state + x
        let nextProgram = next(newState)
        syncInterpreter newState nextProgram
    | Next(Multiply(x, next)) ->
        let newState = state * x
        let nextProgram = next(newState)
        syncInterpreter newState nextProgram

let rec asyncInterpreter state program = 
    match program with
    | Result x -> async { return x } 
    | Next(Add(x, next)) -> async {
        let newState = state + x
        let nextProgram = next(newState)
        return! asyncInterpreter newState nextProgram
        }
    | Next(Multiply(x, next)) -> async {
        let newState = state * x
        let nextProgram = next(newState)
        return! asyncInterpreter newState nextProgram
        }
    
let syncResult = syncInterpreter 0 program
let asyncResult = asyncInterpreter 0 program |> Async.RunSynchronously
printfn "sync: %d" syncResult
printfn "async: %d" asyncResult
