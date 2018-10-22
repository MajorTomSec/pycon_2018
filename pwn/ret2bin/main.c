/* gcc -o bofbof main.c -fno-stack-protector -no-pie -z noexecstack -z relro -m32 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

char flag[64];

void get_input(char *buf, const char *text)
{
  printf("%s : ", text);
  fgets(buf, 256, stdin);
}

int main(int argc, char *argv[])
{
  char input[64];
  FILE *f = NULL;

  setvbuf(stdout, NULL, _IONBF, 0);

  f = fopen(".flag", "rb");
  fread(flag, 64, 1, f);
  fclose(f);

  get_input(input, "Flag");

  if (memcmp(input, flag, 64) == 0)
    printf("Correct !\n");
  else
    printf("Wrong!\n");

  return 0;
}
