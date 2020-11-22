# HazineCaseStudy


## Prerequisites

* [netcoreapp3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) Runtime
* or Docker

## Running with dotnet


```
# dotnet run <txt file path> [thread count] 

dotnet run test-data/file50mb.txt 6 
 ```

--- 
## Running with docker

### image build

```docker build -t hazine-cs .```

### run

```docker run --rm hazine /test-data/file1.txt```
