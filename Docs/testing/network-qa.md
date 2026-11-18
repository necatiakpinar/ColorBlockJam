# Network QA

Verifies [network.md](../General/network.md) and R-43, R-44. `⚙` = automated (test named),
`👁` = manual.

## Test harness

- Cut the network: airplane mode on a device; in the editor, turn `NetworkConfig.BypassInEditor` off and
  disconnect the machine, or point `NetworkConfig.ProbeUrl` at an address that does not answer.
- Wi-Fi without internet: a phone hotspot with its own mobile data off.

## Checklist

### Connectivity and trusted time (T-44)

- [x] ⚙ Only changes are announced; offline probes come sooner; resume and Retry probe at once — `ConnectivityServiceTests`
- [x] ⚙ Trusted time comes from the server's Date header and moves on with monotonic time — `TrustedClockTests`
- [ ] 👁 In play mode the console logs `Connectivity: online` once, and the trusted time matches the real time within 2 s.
- [ ] 👁 Cutting the network flips the game offline within 15 s; reconnecting flips it back within 8 s.
- [ ] 👁 Moving the device clock a day ahead does not change the trusted time.
