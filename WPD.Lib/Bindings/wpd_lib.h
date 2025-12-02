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

#include "../../../NativeLogger/native_logger.h"    
    
    
    
#ifdef __cplusplus
}
#endif

#endif