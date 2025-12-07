#ifndef WPDLIB_SHARED_H
#define WPDLIB_SHARED_H

/* =======================================================================
 * Platform & Visibility Macros
 * ======================================================================= */
#if defined(_WIN32)
    #ifdef WPDLIB_EXPORTS
        #define WPDLIB_API __declspec(dllexport)
    #else
        #define WPDLIB_API __declspec(dllimport)
    #endif
    #define NATIVE_CDECL __cdecl
#else
    #define WPDLIB_API __attribute__((visibility("default")))
    #define NATIVE_CDECL __cdecl
#endif

#ifdef __cplusplus
extern "C" {
#endif

#include "native_logger.h"

    typedef enum
    {
        InvalidArgs = -1, 
        Success = 0,
        Exception = 1
    } Status;
    
    WPDLIB_API Status wpd_repack(char* inputWpdDir);
    WPDLIB_API Status wpd_unpack(char* inputWdpFile);
    
    
#ifdef __cplusplus
}
#endif

#endif