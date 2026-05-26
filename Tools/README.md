# Local Docker Tools

Run commands from the solution root.

Prepare ElasticMQ, create the local queue, and build the service image:

```powershell
powershell -ExecutionPolicy Bypass -File Tools\setup-local-docker.ps1
```

Prepare everything and run the service immediately:

```powershell
powershell -ExecutionPolicy Bypass -File Tools\setup-local-docker.ps1 -RunService
```

Run the service again without rebuilding:

```powershell
powershell -ExecutionPolicy Bypass -File Tools\run-service-local.ps1
```

Read messages:

```powershell
powershell -ExecutionPolicy Bypass -File Tools\read-local-queue.ps1
```

Check queue counts:

```powershell
powershell -ExecutionPolicy Bypass -File Tools\queue-count.ps1
```
