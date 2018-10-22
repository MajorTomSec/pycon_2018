/* gcc -o format_string main.c -no-pie -z relro -m32 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

void (*func)(void) =  NULL;

void shell()
{
  system("/bin/bash");
}

void goodbye()
{
  puts("Done ! Exiting...");
  exit(0);
}

int main(int argc, char *argv[])
{
  FILE *f = NULL;
  char message[64];

  func = goodbye;

  setvbuf(stdout, NULL, _IONBF, 0);

  printf("Message to echo : ");
  fgets(message, 64, stdin);

  printf(message);

  func();

  return 0;
}
