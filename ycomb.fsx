let rec Y f x = f (Y f) x

let Y' f x = 
    let r = ref Unchecked.defaultof<'a -> 'b>
    r := (fun x -> f !r x)
    f !r x

let fib f x = if x <= 1 then x else f(x - 1) + f(x - 2)

Y fib 10 |> printfn "Fib 10: %d"
Y' fib 10 |> printfn "Fib 10: %d"
