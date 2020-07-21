#Example how to use Eureka service discovery

I created this becasue users are having trouble getting Eureka to work with Ocelot, hopefully this helps. 
Please review the implementation of the individual servics to understand how everything fits together.

##Instructions

1. Get Eureka installed and running...

    ```
    $ git clone https://github.com/spring-cloud-samples/eureka.git
    $ cd eureka
    $ mvnw spring-boot:run
    ```
    Leave the service running
    
2. Get Downstream service running and registered with Eureka

    ```
    cd ./DownstreamService/
    dotnet run
    ```

    Leave the service running

3. Get API Gateway running and collecting services from Eureka

    ```
    cd ./ApiGateway/
    dotnet run
    ```

    Leave the service running

4. Make a http request to http://localhost:5000/category you should get the following response

    ```json
    ["category1","category2"]
    ```