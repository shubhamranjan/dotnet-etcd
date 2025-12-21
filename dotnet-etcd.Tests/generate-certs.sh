#!/bin/bash

# Script to generate self-signed certificates for etcd SSL testing
# Run this script in the dotnet-etcd.Tests directory

set -e
export MSYS_NO_PATHCONV=1

CERT_DIR="./certs"
mkdir -p $CERT_DIR

# Generate CA
openssl genrsa -out $CERT_DIR/ca-key.pem 2048
cat > $CERT_DIR/ca-ext.cnf <<EOF
[req]
distinguished_name = req_distinguished_name
x509_extensions = v3_ca
prompt = no

[req_distinguished_name]
CN = etcd-ca

[v3_ca]
basicConstraints = critical,CA:TRUE
keyUsage = critical, digitalSignature, cRLSign, keyCertSign
subjectKeyIdentifier = hash
authorityKeyIdentifier = keyid:always,issuer
EOF

openssl req -x509 -new -nodes -key $CERT_DIR/ca-key.pem \
  -days 3650 -out $CERT_DIR/ca.pem \
  -config $CERT_DIR/ca-ext.cnf

# Generate server certificate  
openssl genrsa -out $CERT_DIR/server-key.pem 2048
openssl req -new -key $CERT_DIR/server-key.pem \
  -out $CERT_DIR/server.csr \
  -subj "/CN=localhost"

# Create extensions file for SAN
cat > $CERT_DIR/server-ext.cnf <<EOF
basicConstraints=critical,CA:FALSE
keyUsage=critical,digitalSignature,keyEncipherment
extendedKeyUsage=serverAuth
subjectKeyIdentifier=hash
authorityKeyIdentifier=keyid,issuer:always
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
DNS.2 = etcd-ssl
IP.1 = 127.0.0.1
EOF

openssl x509 -req -in $CERT_DIR/server.csr \
  -CA $CERT_DIR/ca.pem -CAkey $CERT_DIR/ca-key.pem \
  -CAcreateserial -out $CERT_DIR/server.pem \
  -days 3650 -extfile $CERT_DIR/server-ext.cnf

# Generate client certificate (optional, for mutual TLS)
openssl genrsa -out $CERT_DIR/client-key.pem 2048
openssl req -new -key $CERT_DIR/client-key.pem \
  -out $CERT_DIR/client.csr \
  -subj "/CN=etcd-client"

openssl x509 -req -in $CERT_DIR/client.csr \
  -CA $CERT_DIR/ca.pem -CAkey $CERT_DIR/ca-key.pem \
  -CAcreateserial -out $CERT_DIR/client.pem \
  -days 3650

# Cleanup CSR files
rm $CERT_DIR/*.csr $CERT_DIR/server-ext.cnf $CERT_DIR/ca-ext.cnf

echo "Certificates generated successfully in $CERT_DIR"
echo "CA: ca.pem"
echo "Server: server.pem, server-key.pem"
echo "Client: client.pem, client-key.pem"
