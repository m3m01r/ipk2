import socket

###
### This program is simulation of a server for IPK25CHAT protocol. UDP variant
###

def create_socket(port):
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind(('0.0.0.0', port))
    print(f"Socket is opened on port {sock.getsockname()[1]}")
    return sock

# Creating socket on port 4567
sock = create_socket(4567)
switched = False
response_counter = 0  

#Here are defined templates for all kind of messages that can be send.
### confirm - it's in a code defined later, because it should be dynamic and change depending on a messageid of received packet. 
ping = b"\xFD\x00\x00"
reply = b"\x01\x00\x00\x01\x00\x00\x77\x88\x00"
notreply = b"\x01\x00\x00\x00\x00\x00\x77\x88\x00"
msg = b"\x04\x00\x00\x77\x00\x77\x88\x00"
bye =  b"\xFF\x00\x00\x77\x00"
err = b"\xFE\x00\x00\x77\x00\x77\x88\x00"
#

while True:
    print(f"\nWaiting message on port {sock.getsockname()[1]}...")
    data, addr = sock.recvfrom(1024)
    print(f"Received {addr}: {data}")

    response = bytes([0x00, 0x00, response_counter % 256])#sending CONFIRMS
    sock.sendto(response, addr)
    print(f"First response sent: {response}")

    response_counter += 1 

    if not switched:
        new_sock = create_socket(0)
        new_port = new_sock.getsockname()[1]

        followup_response = b"\x01\x00\x00\x01\x00\x00\x77\x88\x00"
        new_sock.sendto(followup_response, addr)
        print(f"Sended from new port: '{followup_response.decode(errors='ignore')}'")

        sock.close()
        sock = new_sock
        switched = True
    
    response = bytes([0x04, 0x00, 0x01, 0x77, 0x88, 0x00, 0x77, 0x88, 0x00])
    sock.sendto(response, addr)