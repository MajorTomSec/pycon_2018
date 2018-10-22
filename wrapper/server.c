#define _GNU_SOURCE
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/wait.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <dlfcn.h>
#include <string.h>
#include <pwd.h>
#include <grp.h>
#include <link.h>
#include <dlfcn.h>

#define BACKLOG 32
#define logf(...) fprintf(stderr, __VA_ARGS__)

struct sockaddr_in client;

int init_socket(int port) {
    // create socket
    int sockfd = socket(AF_INET, SOCK_STREAM, 0);
    if (sockfd < 0) {
        perror("Unable to create socket.");
        exit(EXIT_FAILURE);
    }
    // reusable sockfd
    int val = 1;
    if (setsockopt(sockfd, SOL_SOCKET, SO_REUSEADDR, (void*) &val, sizeof val) < 0) {
        perror("Unable to set socket option REUSEADDR.");
        exit(EXIT_FAILURE);
    }
    // bind socket
    struct sockaddr_in addr;
    addr.sin_family = AF_INET;
    addr.sin_addr.s_addr = INADDR_ANY;
    addr.sin_port = htons(port);
    if (bind(sockfd, (struct sockaddr*) &addr, sizeof(addr)) < 0) {
        perror("Unable to bind socket.");
        exit(EXIT_FAILURE);
    }
    // set backlog
    if (listen(sockfd, BACKLOG) < 0) {
        perror("Unable to set backlog.");
        exit(EXIT_FAILURE);
    }
    return sockfd;
}

void drop_privs() {
    char* user = "challenge";
    struct passwd* pw = getpwnam(user);
    if (pw == NULL) {
        logf("User \"%s\" does not exist", user);
        exit(EXIT_FAILURE);
    }
    if (setgroups(0, NULL) != 0) {
        perror("Error on setgroups");
        exit(EXIT_FAILURE);
    }
    if (setgid(pw->pw_gid) != 0) {
        perror("Error on setgid");
        exit(EXIT_FAILURE);
    }
    if (setuid(pw->pw_uid) != 0) {
        perror("Error on setuid");
        exit(EXIT_FAILURE);
    }
}

int main(int argc, char* argv[]) {

    if (argc < 3) {
      printf("usage: %s [binary] [port]\n", argv[0]);
      return EXIT_FAILURE;
    }

    int sockfd = init_socket(atoi(argv[2]));
    logf("Server listening on port %d\n", atoi(argv[2]));

    if (signal(SIGCHLD, SIG_IGN) == SIG_ERR) {
        perror("Error setting SIGCHILD handler.");
        return EXIT_FAILURE;
    }

    while (1) {
        socklen_t client_len = sizeof(client);
        int client_fd = accept(sockfd, (struct sockaddr*) &client, &client_len);
        if (client_fd < 0) {
            perror("Error creating socket for incoming connection");
            exit(EXIT_FAILURE);
        }
        logf("New connection from %s on port %d\n", inet_ntoa(client.sin_addr), htons(client.sin_port));

        int pid = fork();
        if (pid < 0) {
            perror("Unable to fork");
            exit(EXIT_FAILURE);
        }

        if (pid == 0) { // client
            alarm(300);
            close(sockfd);

            dup2(client_fd, 0);
            dup2(client_fd, 1);

            //drop_privs();

            execl(argv[1], argv[1], NULL);

            close(client_fd);
            logf("%s:%d disconnected\n", inet_ntoa(client.sin_addr), htons(client.sin_port));
            exit(EXIT_SUCCESS);
        } else {        // server
            logf("%s:%d forked new process with pid %d\n", inet_ntoa(client.sin_addr), htons(client.sin_port), pid);
            close(client_fd);
        }

    }

    return EXIT_SUCCESS;
}
