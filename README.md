# GroundControlStation


Ground Control Station (GCS) using .Net 10 +, that can control a swarm of 1 or more drones, using ardupilot, ELRS/MavLink, WiFi, indoors. 
There are to be no VTX, instead I plan on viewing them from the GCS. 

Eventually a .Net MAUI App dedicated to configuring, planning, and controlling this swarm scenario wil be added.

Indoor multi-drone GCS for ArduPilot-compatible vehicles over MAVLink/WiFi/ELRS, without FPV VTX.
DroneBridge on the ESP32-C6 operates as a transparent telemetry bridge (routing standard MAVLink packets over UDP or TCP via WiFi/ESP-NOW), 

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
	
