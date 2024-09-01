# Core Affinity Service

Core Affinity Service is auto process affinity configuration software for X3D processors in 2CCD configuration

## Create Service

1. Download from Releases.
1. Run cmd.exe as admin.
1. ```sc create "Core Affinity Service" start=auto binPath="C:\Path\To\CoreAffinityService.exe"```

## Affinity Configuration

Configure the target to set affinity in `appsettings.json`.

`ServiceAffinityMask` is as this software affinity mask.

### AffinityProfiles section

Define a pair of profile name and AffinityMask

### TargetProcessRule section

* ProcessName
  * Target process name
* Profile
  * Affinity profile name
* DelayDuration
  * Delay seconds on apply affinity.

### TargetFolderRule section

* FolderPath
    * Target process contain folder path
* Profile
    * Affinity profile name
* DelayDuration
    * Delay seconds on apply affinity.

### Example

```
"AffinityConfiguration": 
  {
    "ServiceAffinityMask" : "0x0000000080000000",
    "AffinityProfiles": [
      {
        "Name": "All",
        "AffinityMask": "0x00000000FFFFFFFF"
      },
      {
        "Name": "CCD0",
        "AffinityMask": "0x000000000000FFFF"
      },
      {
        "Name": "CCD1",
        "AffinityMask": "0x00000000FFFF0000"
      }
    ],
    "TargetProcessRules": [
      {
        "ProcessName": "EscapeFromTarkov",
        "Profile": "CCD0",
        "DelayDuration": 30
      }
    ],
    "TargetFolderRules": [
      {
        "FolderPath": "C:\\SteamLibrary",
        "Profile": "CCD0",
        "DelayDuration": 30
      }
    ]
  }
```


