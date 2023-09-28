# CUSTOMIZE: Change the following variables to suit your needs
DOC_DIR := doc

.PHONY: protoc
protoc:
	mkdir -p $(DOC_DIR) && \
	protoc -I ./pb --csharp_out=./ --doc_out=./$(DOC_DIR) --doc_opt=html,index.html pb/*.proto