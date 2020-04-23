/*
 * Program entry point.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#ifdef WIN32
#include <sensateiot/getopt.h>
#else
#include <unistd.h>
#endif

#include <sensateiot/application.h>

static void help()
{
}

int main(int argc, char* argv[])
{
	int opt;
	size_t len;
	char* path;

	path = NULL;

	while((opt = getopt(argc, argv, "c:h")) != -1) {
		switch(opt) {
		case 'c':
			len = strlen(optarg);
			path = malloc(len + 1);
			memcpy(path, optarg, len + 1);
			break;

		case 'h':
			help();
			return -EXIT_SUCCESS;

		default:
			return -EXIT_FAILURE;
		}
	}

	if(path == NULL) {
		fprintf(stderr, "Configuration file path not set.\n");
		return -EXIT_FAILURE;
	}

	CreateApplication(path);
	free(path);

	return -EXIT_SUCCESS;
}
