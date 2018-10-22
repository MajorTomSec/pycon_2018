#include <stdio.h>
#include <stdlib.h>

unsigned char j = '_';
unsigned char *c = "\xcf\x26\xfc\x30\xf1\x00\xe4\x11\xaf\x2f\xfa\x00\xae\x11\xec\x2b\xed\x0a\xfc\x2b\xae\x6f\xd1\x2c\xc0\x6b\xed\x3a\xc0\x1d\xaf\x2d\xae\x31\xd8\x22";

unsigned char m(unsigned char j)
{
  int i = 1;
  unsigned char o = j;
  for (i = 0; i < j; i++)
  {
    j ^= (j >> 9) + 1;
    if (j != o)
      j ^= 1;
  }
  return j - 1;
}

int main(int argc, char *argv[])
{
  /* Pycon_{N0pe_1NstrUct10Ns_4re_B0r1nG} */

  int f = 0, i;
  unsigned char *input = malloc(50 * sizeof(char) + 1);

  if(!input) {
    printf("Erreur lors de l'allocation memoire.\n");
    exit(1);
  }

  fgets(input, 50, stdin);

  for (i = 0; i < 36; i++) {
    j++;
    if ((unsigned char)(~(input[i]) ^ j) != c[i]) {
      f = 1;
      break;
    }
    j = m(j + 1);
    j = ~j;
  }

  if (f)
    printf("Try harder...\n");
  else
    printf("Correct!\n");

fail:
  free(input);

  return f;
}
