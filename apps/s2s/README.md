# Introduction

This sample demonstrates service-to-service communication using the `OneImlx.Terminal` framework via TCP, UDP, HTTP, and gRPC protocols.

## Overview

**Test Server**: Standalone command processor supporting Console, TCP, UDP, and gRPC modes.

**Test API Server**: Hybrid ASP.NET Web API that hosts both REST endpoints and Terminal HTTP router concurrently in a single application.

**Test Client**: Console-based client for manual command testing across all supported protocols.

> **NOTE**: The client is designed for manual testing during development. In production scenarios, both client and server can operate as automated services.

## Configuration and Setup

### Test Server

- **Modes**: Console, TCP, UDP, gRPC
- **Configuration**: `appsettings.json` (IP, port, mode)
- **Logging**: Serilog to console

### Test API Server

Hybrid architecture hosting ASP.NET Web API and Terminal HTTP router:

- **Endpoints**:
  - `/api/*` - Standard REST API endpoints (`/api/pingdotnet`, `/api/pingterminal`)
  - `/oneimlx/terminal/httprouter` - Terminal command processing

- **Setup**:
  - Uses `MapTerminalHttp()` to register Terminal endpoint
  - Uses `RunTerminalRouterBackgroundAsync()` to start Terminal router
  - Terminal services injectable into ASP.NET controllers via DI
  - Default port: 5006

- **Use Cases**: Add Terminal commands to existing ASP.NET apps, provide dual REST/Terminal interfaces, query Terminal status from API endpoints

### Test Client

- **Protocols**: TCP (`send tcp`), UDP (`send udp`), HTTP (`send http`), API HTTP (`send apihttp`), gRPC (`send grpc`)
- **Configuration**: `appsettings.json` - set server IP and port (default: `localhost:5006`)

## Testing and Validation

Configure Visual Studio startup projects to launch server and client concurrently.

**Option 1 - Standalone**: Test Server + Test Client
- Supports: TCP, UDP, HTTP, gRPC modes
- Configure mode in Test Server's `appsettings.json`

**Option 2 - Hybrid**: Test API Server + Test Client  
- Runs ASP.NET API and Terminal router concurrently
- Use `send apihttp` to test both Terminal commands and REST API calls
- Listens on port 5006 for both API requests and Terminal commands

## Test Execution

Monitor console outputs to verify command processing. 

**Hybrid Mode (`send apihttp`)** demonstrates:
- Terminal commands to `/oneimlx/terminal/httprouter`
- REST API calls to `/api/pingdotnet` and `/api/pingterminal`
- Concurrent processing with metrics (commands sent, API requests, execution time)

## Troubleshooting

- Check console outputs for error messages
- Verify network connectivity and port configurations
- Use network monitoring tools for packet inspection

## Best Practices

- **Error Handling**: Implement robust exception handling and logging
- **Security**: Use HTTPS in production, consider encryption for TCP/UDP
- **Performance**: Monitor resource usage under expected loads
- **Hybrid Integration**:
  - Register Terminal services in ASP.NET DI container
  - Use `MapTerminalHttp()` and `RunTerminalRouterBackgroundAsync()` 
  - Access Terminal services (router, processor, console) via DI in controllers

## Conclusion

The `OneImlx.Terminal` framework provides flexibility for both standalone servers and ASP.NET-integrated deployments. The Test API Server demonstrates how to add Terminal command capabilities to existing ASP.NET applications without replacing existing REST APIs, enabling hybrid architectures with dual communication interfaces.
