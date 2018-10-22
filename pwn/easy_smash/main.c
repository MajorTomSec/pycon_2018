/* gcc -o bofbof main.c -fno-stack-protector -no-pie -z noexecstack -z relro -m32 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

void shell()
{
  system("/bin/sh");
}

void list_files()
{
  system("ls");
}

int main(int argc, char *argv[])
{
  char input[64], c;
  FILE *f = NULL;

  setvbuf(stdout, NULL, _IONBF, 0);

  list_files();

  printf("File to read : ");
  fgets(input, 100, stdin);

  input[strlen(input) - 1] = '\0';

  if (strstr(input, ".flag") != NULL) {
    puts("That's a nope !");
    goto _exit;
  }

  if ((f = fopen(input, "rb")) == NULL) {
    puts("File does not exist. :(");
    goto _exit;
  }

  while (fread(&c, 1, 1, f)) { putchar(c); }
  fclose(f);

_exit:
  return 0;
}
