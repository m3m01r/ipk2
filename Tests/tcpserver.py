import socket
import time

###
### This program is simulation of a server for IPK25CHAT protocol. TCP variant
###

HOST = '0.0.0.0'
PORT = 4567

def start_tcp_server():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen(5) 

    #Here are defined templates for all kind of messages that can be send.
    malformed = f"Malformed message\r\n".encode('ascii')
    reply = f"REPLY OK IS everything is ok\r\n".encode('ascii')
    notreply = f"REPLY NOK IS something is bad\r\n".encode('ascii')
    msg = f"MSG FROM Server IS Message\r\n".encode('ascii')
    bye = f"BYE FROM Server\r\n".encode('ascii')
    err = f"ERR FROM Server IS Error occured\r\n".encode('ascii')
    #
    print(f"TCP server is listening on {HOST}:{PORT}")

    while True:
        client_socket, addr = server_socket.accept()
        print(f"\nClient connected: {addr}")

        try:
            while True:
                data = client_socket.recv(1024)
                if not data:
                    print(f"Client {addr} disconnected")
                    break

                print(f"Rececived from {addr}: {data}")
                # time.sleep(0.2)
                # response = malformed
                # client_socket.sendall(response)
                # print(f"Sent: {response}")

        except Exception as e:
            print(f"Error in communication: {e}")
        finally:
            client_socket.close()

if __name__ == "__main__":
    start_tcp_server()