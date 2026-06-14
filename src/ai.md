Ground Control Station


MavLink:

I want to create a Ground Control Station (GCS) using .Net 10 +, that can control a swarm of 1 or more drones, using ardupilot, ELRS/MavLink, WiFi, indoors. 
There are to be no VTX, instead I plan on viewing them from the GCS. 

I would also like to build a Flight Controller module that can interface with the Ardupilot-compatible FC and ESC, and a separate module for Betaflight-compatible FC and ESC.

I have already some DroneBridge ESP32-C6, that are to be integrated with Ardupilot compatible FC and ESC 

I will eventually scope it to be an App dedicated to configuring, planning, and controlling this swarm scenario.



Indoor multi-drone GCS for ArduPilot-compatible vehicles over MAVLink/WiFi/ELRS, without FPV VTX.


GCS App
  - vehicle discovery
  - MAVLink connection management
  - per-drone state/heartbeat
  - arming/disarming
  - mode control
  - indoor mission planner
  - guided movement commands
  - safety/geofence/kill switch
  - telemetry dashboard
  - parameter subset editor
  - log/event view
	


## 1. Ground Control Station (GCS) - .NET 10:
Building a custom Ground Control Station (GCS) in **.NET 10** for drone swarms requires a highly performant, asynchronous network layer. 

Because DroneBridge on the ESP32-C6 operates as a transparent telemetry bridge (routing standard MAVLink packets over UDP or TCP via WiFi/ESP-NOW), 
your GCS needs a unified abstraction layer capable of switching between a hardware Serial Port (USB) and Network Sockets (UDP/TCP).
The core architecture for the MAVLink transport layer leverages standard .NET 10 networking and system IO abstractions.


Projects:

	DroneGcs.Core
	  MAVLink messages
	  vehicle state
	  command model
	  mission model
	  safety model

	DroneGcs.Transport
	  UDP
	  TCP
	  serial
	  ELRS/MAVLink bridge
	  DroneBridge adapter

	DroneGcs.Simulator
	  fake vehicles for testing

	DroneGcs.Core.Tests



Phase 1: single drone
  WiFi MAVLink connection
  heartbeat/status
  arm/disarm
  mode change
  live telemetry
  guided commands
  emergency stop

Phase 2: two drones
  distinct system IDs
  per-drone command routing
  synchronized status view
  group commands with confirmation
  collision/safety envelope

Phase 3: indoor planning
  room/map coordinate system
  waypoints in local coordinates
  simple choreography/missions
  constraints: max height, max speed, keep-out zones

Phase 4: autonomy support
  companion computer / external positioning
  formation behavior
  scripts



### Integration with DroneBridge & Ardupilot
 * **MAVLink Parsing Engine:** Instead of manual byte slicing, install an established parser like Asv.Mavlink via NuGet, or auto-generate strongly-typed messages using the official MavGen tool compiling down to a C# class library.
 * **Framing (v1 vs v2):** Ensure your parsing layer checks for the MAVLink v2 magic marker (0xFD) or v1 marker (0xFE). Ardupilot targets v2 for telemetry-rich profiles.
 * **DroneBridge Transparent Mode:** Because DroneBridge passes packets through raw, your UdpMavLinkTransport will interact directly with the Ardupilot Flight Controller seamlessly, just as if it were tethered via USB.

Are you looking to use an open-source parsing library for the generated MAVLink definitions, or do you intend to write a custom binary frame-parser for this architecture?


## 2. FlightController - Ardupilot:
As part of this system, I would also like to build a Flight Controller module that can interface with the Ardupilot-compatible FC and ESC. 
This module would be responsible for sending control commands to the drones and receiving telemetry data.

 * **MAVLink Message Handling:** Implement a message dispatcher that routes incoming MAVLink messages to appropriate handlers based on their message ID. This allows for modular handling of different message types (e.g., telemetry, commands, parameters).
 * **Command Interface:** Create a command interface that allows the GCS to send MAVLink commands to the drones. This could include takeoff, landing, waypoint navigation, and other mission commands.
 * **Telemetry Display:** Design a user interface that displays real-time telemetry data from the drones, such as GPS coordinates, altitude, battery status, and sensor readings. This can be achieved using WPF or WinForms for a desktop application.
 * **Mission Planning:** Implement a mission planning module that allows users to create and upload missions to the drones. This could include waypoints, geofences, and other mission parameters.


## 3. FlightController - Betaflight:
As part of this system, I would also like to build a Flight Controller module that can interface with the Betaflight-compatible FC and ESC. 
This module would be responsible for sending control commands to the drones and receiving telemetry data.




