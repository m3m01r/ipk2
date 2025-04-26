
all:
	@dotnet build Project/ipk25chat-client.csproj
	@cd Project && dotnet publish -c Release -r linux-x64 --self-contained true  -p:PublishSingleFile=true -o .
	@cd Project && mv -f ipk25chat-client ..

clean:
	@rm -f ipk25chat-client
	@rm -f -r Project/bin
	@rm -f -r Project/obj
	@rm -f Project/ipk25chat-client.pdb
