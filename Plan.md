# Building an HTTP Server from TCP in C# - A Conceptual Guide

## Chapter 1 - HTTP Streams

Understanding HTTP as a stream-based protocol is fundamental to building a server from scratch.

### 1.1 What is a Stream?
A stream is a continuous flow of data that can be read sequentially. HTTP operates over TCP streams, meaning data arrives as a sequence of bytes rather than all at once.

### 1.2 Stream Characteristics
Streams have no inherent message boundaries. You must parse the incoming bytes to determine where one HTTP message ends and another begins.

### 1.3 Reading from Streams
Your server will need to read data incrementally, handling cases where a complete HTTP message hasn't arrived yet, or where multiple messages arrive together.

### 1.4 Writing to Streams
Responses must be written back to the same stream, formatted according to HTTP specifications so clients can understand them.

---

## Chapter 2 - TCP

TCP is the transport layer protocol that HTTP builds upon. Your server starts here.

### 2.1 Creating a TCP Listener
Set up a listener that binds to a specific IP address and port (typically port 80 for HTTP or 8080 for testing).

### 2.2 Accepting Connections
The listener waits for incoming client connections. When a client connects, accept the connection to establish a two-way communication channel.

### 2.3 Getting the Network Stream
Once connected, obtain the network stream from the TCP client. This stream is your interface for reading requests and writing responses.

### 2.4 Handling Multiple Connections
Decide whether to handle connections sequentially or concurrently. For a real server, you'll want to handle multiple clients simultaneously using threads or async operations.

### 2.5 Connection Lifecycle
Manage when to keep connections open (persistent connections) versus when to close them after each request.

---

## Chapter 3 - Requests

An HTTP request is a structured message sent by the client to your server.

### 3.1 Request Structure Overview
Every HTTP request consists of three parts: the request line, headers, and an optional body. These are separated by specific delimiters.

### 3.2 Reading the Raw Request
Read bytes from the network stream into a buffer. You'll need to accumulate data until you have at least the request line and headers.

### 3.3 Finding Message Boundaries
HTTP uses CRLF (carriage return + line feed: \r\n) to separate lines, and a double CRLF (\r\n\r\n) to separate headers from the body.

### 3.4 Parsing Strategy
Decide whether to parse the request as you read it, or buffer the entire request first. Buffering is simpler but uses more memory.

### 3.5 Handling Incomplete Requests
If a request isn't complete, continue reading from the stream until you have enough data to process.

---

## Chapter 4 - Request Lines

The request line is the first line of every HTTP request and contains critical information.

### 4.1 Request Line Format
The format is: METHOD PATH HTTP/VERSION (for example: GET /index.html HTTP/1.1).

### 4.2 Extracting the HTTP Method
Parse the first word before the space. Common methods are GET, POST, PUT, DELETE, HEAD, OPTIONS.

### 4.3 Extracting the Request Path
Parse the text between the first and second spaces. This is the resource the client wants to access.

### 4.4 Extracting the HTTP Version
Parse the version after the second space. You'll typically see HTTP/1.1 or HTTP/1.0.

### 4.5 Handling Query Parameters
If the path contains a question mark, everything after it is query string data that should be parsed separately.

### 4.6 Validation
Ensure the request line is properly formatted. Invalid requests should result in a 400 Bad Request response.

---

## Chapter 5 - HTTP Headers

Headers provide metadata about the request and control how it should be processed.

### 5.1 Header Format
Each header is on its own line in the format: Header-Name: Header-Value.

### 5.2 Reading Headers
After the request line, read subsequent lines until you encounter an empty line (double CRLF), which marks the end of headers.

### 5.3 Parsing Individual Headers
Split each header line at the colon. The part before is the header name, the part after is the value. Trim whitespace from both.

### 5.4 Storing Headers
Store headers in a dictionary or collection for easy lookup. Header names are case-insensitive.

### 5.5 Important Headers to Handle
Pay special attention to Content-Length (tells you how many body bytes to read), Host (required in HTTP/1.1), Connection (controls keep-alive), and Transfer-Encoding (affects body reading).

### 5.6 Multi-Line Headers
Some headers can span multiple lines. Lines starting with whitespace continue the previous header value.

---

## Chapter 6 - HTTP Body

The request body contains data sent by the client, typically with POST or PUT requests.

### 6.1 Determining if a Body Exists
Check if the Content-Length header is present or if Transfer-Encoding is set to chunked.

### 6.2 Reading a Fixed-Length Body
If Content-Length is specified, read exactly that many bytes from the stream after the headers.

### 6.3 Handling Large Bodies
For large bodies, consider streaming the data rather than loading it all into memory at once.

### 6.4 Content-Type Awareness
The Content-Type header tells you how to interpret the body (for example, application/json, application/x-www-form-urlencoded, multipart/form-data).

### 6.5 Body Parsing
Depending on the Content-Type, you may need to parse the body differently (JSON parsing, form data parsing, etc.).

---

## Chapter 7 - HTTP Responses

After processing a request, your server must send back a properly formatted HTTP response.

### 7.1 Response Structure
Responses consist of a status line, headers, an empty line, and an optional body.

### 7.2 Writing the Status Line
Format: HTTP/VERSION STATUS_CODE REASON_PHRASE (for example: HTTP/1.1 200 OK). Choose appropriate status codes (200 for success, 404 for not found, 500 for server errors, etc.).

### 7.3 Writing Response Headers
Send headers that describe your response. Always include Content-Length (or use chunked encoding), Content-Type, Date, and Server headers.

### 7.4 The Empty Line
After all headers, write a double CRLF to signal the end of headers and the start of the body.

### 7.5 Writing the Response Body
If your response has content, write it after the empty line. The amount should match your Content-Length header.

### 7.6 Flushing the Stream
Ensure all response data is sent by flushing the network stream after writing.

---

## Chapter 8 - Chunked Encoding

Chunked encoding allows sending responses when you don't know the content length in advance.

### 8.1 When to Use Chunked Encoding
Use this when generating dynamic content, streaming data, or when the full response size isn't known before starting to send.

### 8.2 Setting the Transfer-Encoding Header
Instead of Content-Length, set the Transfer-Encoding header to "chunked".

### 8.3 Chunk Format
Each chunk starts with its size in hexadecimal, followed by CRLF, then the chunk data, then another CRLF.

### 8.4 Writing Chunks
Calculate the size of each chunk you want to send. Write the size in hex, then CRLF, then the data, then CRLF. Repeat for each chunk.

### 8.5 Terminating Chunks
End the response by sending a chunk with size 0, followed by CRLF, then a final CRLF.

### 8.6 Reading Chunked Requests
If a client sends a chunked request body, you must read the chunk size, then read that many bytes, repeating until you encounter a zero-size chunk.

---

## Chapter 9 - Binary Data

Not all HTTP traffic is text. Your server must handle binary data correctly.

### 9.1 Binary vs. Text
Binary data includes images, videos, PDFs, and compressed files. Unlike text, binary data can contain any byte value.

### 9.2 Reading Binary Data
Use byte arrays or buffers rather than strings. Don't assume data is UTF-8 or any text encoding.

### 9.3 Content-Type for Binary
Set appropriate Content-Type headers for binary responses (image/jpeg, application/pdf, application/octet-stream, etc.).

### 9.4 Writing Binary Responses
Write binary data directly to the stream as bytes. Ensure Content-Length accurately reflects the byte count.

### 9.5 Handling File Uploads
For multipart/form-data requests, parse the boundaries and extract binary file data from the request body.

### 9.6 Byte Order and Encoding
Be aware that binary data has no inherent encoding. Handle it as-is without conversion attempts.

---

## Implementation Flow Summary

Your complete HTTP server will follow this flow:

1. Start TCP listener on a port
2. Accept incoming client connection
3. Get the network stream
4. Read bytes until you find the end of headers (double CRLF)
5. Parse the request line to get method, path, and version
6. Parse headers into a collection
7. If a body exists (check Content-Length or Transfer-Encoding), read it
8. Process the request based on method and path
9. Generate a response with appropriate status code
10. Write the status line to the stream
11. Write response headers to the stream
12. Write the empty line separator to the stream
13. Write the response body to the stream (if any)
14. Flush the stream
15. Decide whether to close or keep the connection open
16. Repeat from step 2 for the next request