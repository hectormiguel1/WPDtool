#ifndef IMGBLIB_SHARED_H
#define IMGBLIB_SHARED_H

/* =======================================================================
 * Platform & Visibility Macros
 * ======================================================================= */
#if defined(_WIN32)
    #ifdef IMGBLIB_EXPORTS
        #define IMGBLIB_API __declspec(dllexport)
    #else
        #define IMGBLIB_API __declspec(dllimport)
    #endif
    #define NATIVE_CDECL __cdecl
#else
    #define IMGBLIB_API __attribute__((visibility("default")))
    #define NATIVE_CDECL __cdecl
#endif

#ifdef __cplusplus
extern "C" {
#endif

#include "native_logger.h"
#include "imgblib.h"
    
typedef enum
    {
        InvalidArgs = -1, 
        Success = 0, 
        Exception = 1
    } Status;
    
    typedef enum
    {
        Strict = 0,
        Resize = 1,
    } RepackMode;
    
    typedef enum
    {
        WIN32 = 0, 
        PS3 = 1, 
        X360 = 2
    } Platforms;
    
    typedef enum
    {
        TXB = 0, 
        TXBH = 1,
        VTEX = 2
    } FileExtensions;
    
    //Unpack 
    IMGBLIB_API Status unpack_imgb(char* imgHeaderBlkPtr, char* inFilePtr, char* extractDirPtr, Platforms platform );
    //Repack
    IMGBLIB_API Status repack_imgb_strict(char* imgHeaderBlkPtr, char* outImgbPtr, char* extractedDirPtr, Platforms platform );
    IMGBLIB_API Status repack_imgb_resize(char* tmpImgHeaderBlkPtr,char* imgHeaderBlkPtr, char* outImgbPtr, char* extractedDirPtr, Platforms platform );
    
    
#ifdef __cplusplus
}
#endif

#endif