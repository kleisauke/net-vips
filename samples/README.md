# NetVips Samples
Demonstrates the usage of [NetVips](https://github.com/kleisauke/net-vips).

```bash
cd NetVips.Samples

dotnet run -c Release

# run a particular sample
dotnet run -c Release "Hello world"

# leak testing
dotnet run -c Release "Leak test" "C:/images"
```