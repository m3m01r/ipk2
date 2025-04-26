import socket

###
### This program is needed for checking and giving to output content of a message sent using porotocol IPK25CHAT. UDP
###

def hexdump(data):
    for i in range(0, len(data), 16):
        chunk = data[i:i+16]
        hex_bytes = ' '.join(f'{b:02X}' for b in chunk)
        ascii_bytes = ''.join((chr(b) if 32 <= b <= 126 else '.') for b in chunk)
        print(f"{i:08X}  {hex_bytes:<48}  {ascii_bytes}")

def udp_sniffer(port):
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind(("0.0.0.0", port))
    print(f"Listening on UDP port {port}...\n")

    while True:
        data, addr = sock.recvfrom(4096)
        print(f"\nReceived {len(data)} bytes from {addr}:")
        hexdump(data)

if __name__ == "__main__":
    udp_sniffer(4567)
