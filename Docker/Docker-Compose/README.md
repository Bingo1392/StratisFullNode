**TODO: Nodes don't communicate with each other. There is probably a problem with ports.**

# Private Blockchain

This manual will help you to create your own private blockchain with PoS consensus algorithm. You will create 3 nodes in one network. Than you will mine 1000 blocks with PoW and after that you will setup PoS. Let's do it!

## 1) Build the Docker container:

```
cd ../Stratis.StraxD
docker build --no-cache -tstratisfullnode/private .
```

## 2) Run docker-compose:
```
cd ../Docker-Compose
docker-compose -f 3-node_network-mine.yml -p stratis-private-3-node-network up --force-recreate
```

The docker compose created a small network with 3 nodes. You have access to all of their Swagger APIs:

```
http://localhost:17103/swagger/index.html    // node-1
http://localhost:18103/swagger/index.html    // node-2
http://localhost:19103/swagger/index.html    // node-3
```

## 3) Mine 1000 blocks:

Open node-1's Swagger API on `http://localhost:17103/swagger/index.html` and run `/api/Mining/generate` method with these parameters:

```
{
  "blockCount": 1000
}
```

Now you have to wait until 1000 block will be mined. You can check it in the server log. Just find the last `BlockStore.Height` attribute in the log. If its value is 1000 you are done.

## 4) Setup PoS:

The last step is to setup PoS. It's easy just run again docker-compose but for the second yaml file.

```
docker-compose -f 3-node_network-mine.yml -p stratis-private-3-node-network stop
docker-compose -f 3-node_network-stake.yml -p stratis-private-3-node-network up --force-recreate
```

