/* $Id: lrun.h,v 1.61 2008-06-17 13:34:26 markn Exp $ [MISCCSID] */
/********************************************************
* lrun.h                                                *
* LoadRunner API header file                            *
*********************************************************/
#ifndef __LRUN_H__
#define __LRUN_H__    /* Include me once */

#define LRUN_H_VER		40902

#if defined(cplusplus) || defined(__cplusplus)
extern "C" {
#define PROTO(x)     x
#endif

/* Definitions, Error Codes etc. */

#if defined(unix) || defined(HPUX)|| defined(HP) || defined(HP9) || defined(_IBMR2) || defined(SOLARIS)
#define LR_UNIX
/* General definition for UNIX  */
#endif

#if !defined(PROTO)
#if !defined(LR_UNIX) || defined(__STDC__)
#   define PROTO(x)     x
#else
#   define PROTO(x)     ()
#endif
#endif

#ifndef CCI /* Compiled Vuser */

#include <sys/types.h>

#ifdef LR_UNIX
#define LR_FUNC
#define LR_MSG_FUNC

#include <stdio.h>
#include <stdarg.h>
#define E_NOMEM -5

#ifndef LPCSTR
#define LPCSTR char *
#endif

#ifndef LPSTR
#define LPSTR char *
#endif

#ifndef WINAPI
#define WINAPI
#endif

#ifndef FAR
#define FAR
#endif

#ifndef NEAR
#define NEAR
#endif

#ifndef far
#define far
#endif

#ifndef WORD
#define WORD unsigned short
#endif

#ifndef DWORD
#define DWORD unsigned long
#endif

#ifndef VOID
#define VOID void
#endif

#ifndef LPVOID
#define LPVOID void *
#endif

#else /* i.e. if !defined(CCI) && !defined(LR_UNIX)  */

#include <windows.h>
#include <stddef.h>
# ifdef _WIN32
#	include <winbase.h>
# endif

#ifdef _WIN32
/* LR_FUNC is used for the call-back functions of the compiled Vuser */
#define LR_FUNC	CALLBACK
/* LR_MSG_FUNC is used for the message functions */
#define LR_MSG_FUNC WINAPIV
#else
#define LR_FUNC __export WINAPI
#define LR_MSG_FUNC FAR CDECL __export
#endif

#endif /* LR_UNIX */

#else /* i.e. ifdef CCI */
#define LR_FUNC
#define LR_MSG_FUNC

#ifndef WINAPI
#define WINAPI
#endif

#ifndef far
#define far
#endif

#ifndef FAR
#define FAR
#endif

#ifndef NEAR
#define NEAR
#endif

#ifndef LPCSTR
#define LPCSTR char *
#endif

#ifndef LPSTR
#define LPSTR char *
#endif

#ifndef WINAPIV
#define WINAPIV
#endif

#ifndef CDECL
#define CDECL
#endif

#ifndef cdecl
#define cdecl
#endif

#ifndef NULL
#define NULL 0
#endif

#ifndef __export
#define __export
#define _export
#endif

#ifndef __loadds
#define __loadds
#endif

#ifndef size_t
#define size_t int
#endif

#ifndef PASCAL
#define PASCAL
#endif

#ifndef LPVOID
#define LPVOID void *
#endif

#ifndef TRUE
#define TRUE 1
#endif

#ifndef FALSE
#define FALSE 0
#endif
	
#ifdef _IDA_XL
/* Eyal : Patch  for the sql inspectors
   (they record vuser_run insted of Actions)*/
#define vuser_run Actions
#endif

#ifndef WORD
#define WORD unsigned short
#endif

#ifndef DWORD
#define DWORD unsigned long
#endif

#ifndef VOID
#define VOID void
#endif

#endif /* ifdef CCI */
#define LAST "LAST"

/********************************************/
/********************************************/
/************ Lrun library API **************/
/********************************************/
/********************************************/


/********************************************/
/********************************************/
/**** Vuser transaction related functions ***/
/********************************************/
/********************************************/
/* status values to be used in lr_end_transaction */
#define LR_PASS       0
#define LR_FAIL       1
#define LR_AUTO       2
#define LR_ABORT      3
#define LR_STOP       3 /* The same as LR_ABORT due to analysis request */

#ifndef PASS
#  define PASS 	      0
#endif

#ifndef FAIL
#  define FAIL 	      1
#endif

int WINAPI far lr_start_transaction   PROTO((LPCSTR transaction_name));
int lr_start_sub_transaction          PROTO((LPCSTR transaction_name, LPCSTR trans_parent));
long lr_start_transaction_instance    PROTO((LPCSTR transaction_name, long parent_handle));
int WINAPI lr_start_cross_vuser_transaction		PROTO((LPCSTR transaction_name, LPCSTR trans_id_param)); 


#ifndef LR_PERSONAL
int WINAPI far lr_end_transaction     PROTO((LPCSTR transaction_name, int status));
int lr_end_sub_transaction            PROTO((LPCSTR transaction_name, int status));
int lr_end_transaction_instance       PROTO((long transaction, int status));
int WINAPI lr_end_cross_vuser_transaction	PROTO((LPCSTR transaction_name, LPCSTR trans_id_param, int status));
#endif

/* Distributed transactions APIs: */
typedef char* lr_uuid_t;
/*
 * Generates uuid. Allocate memory for the uuid buffer. Returns a uuid on
 * success or NULL otherwise.
 */
lr_uuid_t lr_generate_uuid();

/*
 * Frees uuid that was created by lr_generate_uuid.
 */
int lr_generate_uuid_free(lr_uuid_t uuid);

/*
 * Generates uuid. Gets the uuid buffer (buf) which must have space for 25 characters.
 * Return 0 on success or negative number otherwise.
 */
int lr_generate_uuid_on_buf(lr_uuid_t buf);

  /*
 argument     : 1 - transaction name.
                2 - correlator, that should be passed also to the corresponding lr_end_distributed_transaction. 
				    In order to create it, you can use another infra API: lr_generate_uuid() and then 
					lr_generate_uuid_free(lr_uuid_t uuid) or lr_generate_uuid_on_buf(lr_uuid_t buf)
					or you can give your own unique string.
                3 - timeout in milliseconds, after that time, the transaction will be closed automatically.
 return value : Zero on success . negative number if an error occurred.
 Usage        : Start transaction by name and correlator for ending it in a different Vuser. 
  */
int lr_start_distributed_transaction  PROTO((LPCSTR transaction_name, lr_uuid_t correlator, long timeout /*in milliseconds */));

  /*
 argument     : 1 - correlator, that should be passed also to the corresponding lr_start_distributed_transaction. 
                2 - transaction ending status - Do not pass LR_AUTO, since it start in different Vuser, so status 
				    must be determined at end.
 return value : Zero on success . negative number if an error occurred.
 Usage        : End transaction by correlator since it was start in a different Vuser. Send a report about the 
                transaction's timing and status. No hierarchy information can be given to this transaction.
  */
int lr_end_distributed_transaction  PROTO((lr_uuid_t correlator, int status));


double lr_stop_transaction            PROTO((LPCSTR transaction_name));
double lr_stop_transaction_instance   PROTO((long parent_handle));


void lr_resume_transaction           PROTO((LPCSTR trans_name));
void lr_resume_transaction_instance  PROTO((long trans_handle));


int lr_update_transaction            PROTO((const char *trans_name));


/* Time wasted that should be removed from all open transactions */
void lr_wasted_time(long time);


/* Transaction "name" took "duration" seconds and finished. The status was "status".*/
int lr_set_transaction(const char *name, double duration, int status);
/* Same as lr_set_transaction function accept the ability to add the transaction parent parent */
long lr_set_transaction_instance(const char *name, double duration, int status, long parent_handle);


int WINAPI lr_user_data_point                      PROTO((LPCSTR, double));
long lr_user_data_point_instance                   PROTO((LPCSTR, double, long));
/* Report data point with flags in order to determine if it should be logged or not */
#define DP_FLAGS_NO_LOG 1
#define DP_FLAGS_STANDARD_LOG 2
#define DP_FLAGS_EXTENDED_LOG 3
int lr_user_data_point_ex(const char *dp_name, double value, int log_flag);
long lr_user_data_point_instance_ex(const char *dp_name, double value, long parent_handle, int log_flag);


int lr_transaction_add_info      PROTO((const char *trans_name, char *info));
int lr_transaction_instance_add_info   PROTO((long trans_handle, char *info));
int lr_dpoint_add_info           PROTO((const char *dpoint_name, char *info));
int lr_dpoint_instance_add_info        PROTO((long dpoint_handle, char *info));


double lr_get_transaction_duration       PROTO((LPCSTR trans_name));
double lr_get_trans_instance_duration    PROTO((long trans_handle));
double lr_get_transaction_think_time     PROTO((LPCSTR trans_name));
double lr_get_trans_instance_think_time  PROTO((long trans_handle));
double lr_get_transaction_wasted_time    PROTO((LPCSTR trans_name));
double lr_get_trans_instance_wasted_time PROTO((long trans_handle));
int    lr_get_transaction_status		 PROTO((LPCSTR trans_name));
int	   lr_get_trans_instance_status		 PROTO((long trans_handle));

/* Set the status of all open transactions to "status"
 * This status will be used for all the transactions that the status
 * in the end_transaction is LR_AUTO
 */
int lr_set_transaction_status(int status);

/* Set the status of a named transaction or an instance one to the parameter 'status'.
 * This status will be used if the transaction status in the lr_end_transaction 
 * operation is LR_AUTO.
 */
int lr_set_transaction_status_by_name(int status, const char *trans_name);
int lr_set_transaction_instance_status(int status, long trans_handle);


typedef void* merc_timer_handle_t;
/* Start calculate time by lr_start_timer() and end it with lr_end_timer.
   lr_end_timer returns the time in seconds.                               */
merc_timer_handle_t lr_start_timer();
double lr_end_timer(merc_timer_handle_t timer_handle);


/********************************************/
/********************************************/
/******** Vuser rendezvous functions ********/
/********************************************/
/********************************************/
/* lr_rendezvous_ex returns one of the followings: */
#define LR_REND_ILLEGAL          -1
#define LR_REND_ALL_ARRIVED       0
#define LR_REND_TIMEOUT	          1
#define LR_REND_DISABLED          2
#define LR_REND_NOT_FOUND         3
#define LR_REND_VUSER_NOT_MEMBER  4
#define LR_REND_VUSER_DISABLED    5
#define LR_REND_BY_USER           6
#define LR_REND_NOCONTACT         512


/* 
 * lr_rendezvous : sets a rendezvous point in the Vuser script
 * Return Value  : 0 on success , negative value on failure
 */
int WINAPI lr_rendezvous  PROTO((LPCSTR rendezvous_name));
/* 
 * lr_rendezvous_ex : sets a rendezvous point in the Vuser script, as lr_rendezvous does
 * Return Value     : the release reason from rendezvous point on success or negative value
 *                    on failure.
 */
int WINAPI lr_rendezvous_ex PROTO((LPCSTR rendezvous_name));



/********************************************/
/********************************************/
/********* Vuser general information ********/
/********************************************/
/********************************************/
char *lr_get_vuser_ip PROTO((void));
void WINAPI lr_whoami PROTO((int *vuser_id, char ** sgroup, int *scid));
LPCSTR	WINAPI lr_get_host_name PROTO((void));
LPCSTR	WINAPI lr_get_master_host_name PROTO((void));

/* attribute getting functions */
long   WINAPI lr_get_attrib_long	PROTO((LPCSTR attr_name));
LPCSTR WINAPI lr_get_attrib_string	PROTO((LPCSTR attr_name));
double WINAPI lr_get_attrib_double      PROTO((LPCSTR attr_name));

char * lr_paramarr_idx(const char * paramArrayName, unsigned int index);
char * lr_paramarr_random(const char * paramArrayName);
int    lr_paramarr_len(const char * paramArrayName);

int	lr_param_unique(const char * paramName);
int lr_param_sprintf(const char * paramName, const char * format, ...);

#ifdef CCI /* Interpreted Vuser */
/* load_dll support */
/* A global parameter that holds the current VM context */
static void *ci_this_context = 0;

#define lr_load_dll(dll_name) \
	ci_load_dll(ci_this_context,(dll_name))

#endif

/* options for lr_continue_on_error */
#define LR_ON_ERROR_NO_OPTIONS                0
#define LR_ON_ERROR_CONTINUE                  1
#define LR_ON_ERROR_SKIP_TO_NEXT_ACTION       2
#define LR_ON_ERROR_SKIP_TO_NEXT_ITERATION    3
#define LR_ON_ERROR_END_VUSER                 4
#define LR_ON_ERROR_CALL_USER_DEFINED_HANDLER 5


void lr_continue_on_error PROTO((int lr_continue));
char * WINAPI lr_decrypt PROTO((const char *EncodedString));


/********************************************/
/********************************************/
/********* Vuser execution utilities ********/
/********************************************/
/********************************************/
/* used to be used in lr_abort_ex and lr_exit.
   kept for backward compatibility.            */
#define LR_ABORT_NO_MATTER_WHAT            0x00000001

/* flags to be used in lr_abort_ex */
#define LR_ABORT_NO_OPTIONS                0x00000000 
#define LR_ABORT_VUSER                     0x00000400
#define LR_ABORT_ACTION                    0x00000002
#define LR_ABORT_ITERATION                 0x00000004
#define LR_ABORT_MAIN_ITERATION            0x00000800
#define LR_ABORT_FORCE                     0x00001000

/* flags to be used in lr_abort_ex for soft kill */
#define LR_ABORT_SOFT_KILL_ITERATION       0x00000010
#define LR_ABORT_SOFT_KILL_ACTION		   0x00000020

#define LR_ABORT_VUSER_FAILED              0x00000008
#define LR_ABORT_VUSER_PASSED              0x00000040
#define LR_ABORT_VUSER_ABORTED             0x00000080
#define LR_ABORT_VUSER_AUTO				   0x00000100

#define LR_EXIT_VUSER                           0
#define LR_EXIT_ACTION_AND_CONTINUE             1
#define LR_EXIT_ITERATION_AND_CONTINUE          2
#define LR_EXIT_VUSER_AFTER_ITERATION           3
#define LR_EXIT_VUSER_AFTER_ACTION              4
#define LR_EXIT_MAIN_ITERATION_AND_CONTINUE     5

void WINAPI lr_abort PROTO((void));
void lr_exit(int exit_option, int exit_status);
void lr_abort_ex PROTO((unsigned long flags));

void WINAPI lr_peek_events PROTO((void));


/********************************************/
/********************************************/
/********* Vuser think time functions *******/
/********************************************/
/********************************************/

#define LR_MAX_THINK_TIME 2000000 /* the maximum think time in seconds */
void WINAPI lr_think_time PROTO((double secs));

/*
 * think time that ignores user RTS. thinks 'secs' time as specified
 */
void lr_force_think_time (double secs);


/********************************************/
/********************************************/
/********** Vuser output functions **********/
/********************************************/
/********************************************/
#define LR_MSG_ERROR     1
#define LR_MSG_SEND      2
#define LR_MSG_LOCATION  4

#define LR_MSG_CLASS_DISABLE_LOG        0
#define LR_MSG_CLASS_STANDARD_LOG       1
#define LR_MSG_CLASS_RETURNED_DATA      2
#define LR_MSG_CLASS_PARAMETERS         4
#define LR_MSG_CLASS_ADVANCED_TRACE     8
#define LR_MSG_CLASS_EXTENDED_LOG       16
#define LR_MSG_CLASS_SENT_DATA          32
#define LR_MSG_CLASS_JIT_LOG_ON_ERROR   512

#define LR_SWITCH_OFF       0
#define LR_SWITCH_ON        1
#define LR_SWITCH_DEFAULT   2

#define MAX_PARAM_LEN 128

int LR_MSG_FUNC lr_msg PROTO((LPCSTR fmt, ...));
int LR_MSG_FUNC lr_debug_message PROTO((unsigned int msg_class,
									    LPCSTR format,
										...));
void WINAPI lr_new_prefix PROTO((int type,
                                 LPCSTR filename,
                                 int line));
int LR_MSG_FUNC lr_log_message PROTO((LPCSTR fmt, ...));
int LR_MSG_FUNC lr_message PROTO((LPCSTR fmt, ...));
int LR_MSG_FUNC lr_error_message PROTO((LPCSTR fmt, ...));
int LR_MSG_FUNC lr_output_message PROTO((LPCSTR fmt, ...));
int LR_MSG_FUNC lr_vuser_status_message PROTO((LPCSTR fmt, ...));
int LR_MSG_FUNC lr_error_message_without_fileline PROTO((LPCSTR fmt, ...));
int LR_MSG_FUNC lr_fail_trans_with_error PROTO((LPCSTR fmt, ...));

/************************************************************/
/* NOTE: All this functions are for internal usage only     */
/* and not for Vuser usage.This functions are designated for*/
/* c++ wrapper of LR API.									*/
/************************************************************/
#ifndef CCI
int LR_MSG_FUNC lr_vmsg (LPCSTR fmt, va_list args_list);
int LR_MSG_FUNC lr_debug_vmsg(unsigned int msg_class, LPCSTR format, va_list args_list);
int LR_MSG_FUNC lr_log_vmessage(LPCSTR fmt, va_list args_list);
int LR_MSG_FUNC lr_vmessage(LPCSTR fmt, va_list args_list);
int LR_MSG_FUNC lr_error_vmessage (LPCSTR fmt, va_list args_list);
int LR_MSG_FUNC lr_output_vmessage (LPCSTR fmt, va_list args_list);
int LR_MSG_FUNC lr_vuser_status_vmessage (LPCSTR fmt, va_list args_list);
int LR_MSG_FUNC lr_error_vmessage_without_fileline (LPCSTR fmt, va_list args_list);
int LR_MSG_FUNC lr_fail_trans_with_error_vmsg (LPCSTR fmt, va_list args_list);
#endif
/********************************************/
/********************************************/
/***** Vuser parameterization functions *****/
/********************************************/
/********************************************/
#define LR_LEFT_BRACE 0
#define LR_RIGHT_BRACE 1
#define DEFAULT_LR_LEFT_BRACE "<"
#define DEFAULT_LR_RIGHT_BRACE ">"

int WINAPI lr_next_row PROTO(( LPCSTR table));
int lr_advance_param PROTO(( LPCSTR param));

#define LR_EVAL_OPT_NONE					 0x00000000
#define LR_EVAL_OPT_DUP_APOST				 0x00000001  /* When subsituting a parameter ref, */
														 /* replace each apostrophe (') with  */
														 /* two consecutive apostrophes ('')  */
#define LR_EVAL_OPT_RESTR_DT				 0x00000002  /* When subsituting a parameter ref. */
														 /* fail if the param is marked as    */
														 /* having a restricted datatype      */
#define LR_EVAL_OPT_NO_PARAM_TO_EMPTY_STRING 0x00000004  /* replace not existing parameter    */
													     /* with empty string			      */
 


LPSTR WINAPI lr_eval_string PROTO((LPCSTR str));
int WINAPI lr_eval_string_ext PROTO((const char *in_str,
                                     unsigned long const in_len,
                                     char ** const out_str,
                                     unsigned long * const out_len,
                                     unsigned long const options,
                                     const char *file,
								     long const line));
void WINAPI lr_eval_string_ext_free PROTO((LPSTR * pstr));

/* ************************************************************************** */
/*  lr_param_increment                                                        */
/* ************************************************************************** */
/*  Input:                                                                    */
/*   dst_name - the name of the destination parameter.                        */
/*   src_name - the name of the source parameter.                             */
/*  Returned value:                                                           */
/*    LR_PARAM_INC_OK              - Success.                                 */
/*    LR_PARAM_INC_ERR_SAVE_FAILED - Failure in saving to the destination.    */
/*    LR_PARAM_INC_ERR_MBCS_IN_SRC - The source includes MB characters.       */
/*    LR_PARAM_INC_ERR_SRC_NOT_NUM - The source is not a pure numeric value.  */
/*    LP_PARAM_INC_ERR_EVAL_FAILED - The evaluation of the source failed.     */
/* ************************************************************************** */
/*  Gets the value of the source parameter,  increments it by 1 and stores it */
/*  to the destination parameter. In case the destination parameter does not  */
/*  exist, it is created.  In case the source  parameters  does not exist or  */
/*  either  of the  parameters  exists but is not a numeric  parameter,  the  */
/*  function fails.                                                           */
/* ************************************************************************** */
int lr_param_increment PROTO((LPCSTR dst_name,
                              LPCSTR src_name));

#define LR_PARAM_INC_OK                   0

#define LR_PARAM_INC_ERR_SAVE_FAILED     -1
#define LR_PARAM_INC_ERR_MBCS_IN_SRC     -2
#define LR_PARAM_INC_ERR_SRC_NOT_NUM     -3
#define LP_PARAM_INC_ERR_EVAL_FAILED     -4


#define LR_SAVE_OPT_NONE          0x00000000
#define LR_SAVE_OPT_UNINIT        0x00000001 /* Indicate param as uninitialized  */
#define LR_SAVE_OPT_NULL          0x00000002 /* Indicate param as NULL           */
#define LR_SAVE_OPT_INVALID       0x00000004 /* Indicate param as having an      */
											 /* an invalid value (e.g., would    */
											 /* necessitate use of RAW in LRD)   */
#define LR_SAVE_OPT_RESTR_DT      0x00000008 /* Indicate param as having a       */
											 /* restricted datatype, which       */
											 /* cannot be used in certain cases  */
											 /* (e.g., DATE in SQL statement)    */
#define LR_SAVE_NO_TRACE          0x10000000 /* do not trace lr_save_value       */
int	WINAPI lr_save_var PROTO((LPCSTR              param_val,
							  unsigned long const param_val_len,
							  unsigned long const options,
							  LPCSTR			  param_name));
int WINAPI lr_save_string PROTO((const char * param_val, const char * param_name));
int WINAPI lr_free_parameter PROTO((const char * param_name));
int WINAPI lr_save_int PROTO((const int param_val, const char * param_name));
int WINAPI lr_save_timestamp PROTO((const char * tmstampParam, ...));
int WINAPI lr_save_param_regexp PROTO((const char *bufferToScan, unsigned int bufSize, ...));

/********************************************/
/********************************************/
/********** Vuser time functions ************/
/********************************************/
/********************************************/
/* lr_save_datetime format
*          %a   day of week, using  locale's  abbreviated  weekday
*               names
*
*          %A   day of week, using locale's full weekday names
*
*          %b   month, using locale's abbreviated month names
*
*          %B   month, using locale's full month names
*
*          %c   date and time as %x %X
*
*          %d   day of month (01-31)
*
*          %H   hour (00-23)
*
*          %I   hour (00-12)
*
*          %j   day number of year (001-366)
*
*          %m   month number (01-12)
*
*          %M   minute (00-59)
*
*          %p   locale's equivalent of  AM  or  PM,  whichever  is
*               appropriate
*
*          %S   seconds (00-59)
*
*          %U   week number of year (01-52), Sunday is  the  first
*               day of the week
*
*          %w   day of week; Sunday is day 0
*
*          %W   week number of year (01-52), Monday is  the  first
*               day of the week
*
*          %x   date, using locale's date format
*
*          %X   time, using locale's time format
*
*          %y   year within century (00-99)
*
*          %Y   year, including century (for example, 1988)
*
*          %Z   time zone abbreviation
*
*          %%   if you actually want % in your output string
*
*     The difference between %U  and  %W  lies  in  which  day  is
*     counted as the first day of the week.  Week number 01 is the
*     first week with four or more January days in it.
*
* Notes:
*    Result string is truncated after MAX_DATETIME_LEN characters.
*    Won't work on dates before Jan 1, 1970
*/
void WINAPI lr_save_datetime PROTO((const char *format, int offset, const char *name));

#define TIME_FMT1       "%H:%M:%S"
#define TIME_FMT2       "%d-%b-%y"
#define TIME_FMT3       "%Y-%m-%d"
#define TIME_FMT4       "%Y-%m-%d %H:%M:%S"
#define TIME_FMT6       "%m/%d/%Y"
#define TIME_FMT7       "%d/%m/%Y"
#define TIME_FMT8       "%Y/%m/%d"
#define DATE_FMT        "%m%d%Y"
/* fix so sccs will not corrupt the definition of TIME_FMT*/
#define HOUR_FMT        "%H"
#define TIME_FMT        "HOUR_FMT%M"
#define ONE_DAY         86400
#define ONE_HOUR        3600
#define ONE_MIN         60
#define DATE_NOW        0
#define TIME_NOW        0
#define MAX_DATETIME_LEN 80

#define LR_MSG_VUSER_STATUS 51

/*****************************************************/
/*****************************************************/
/********** Vuser error context functions ************/
/*****************************************************/
/*****************************************************/

#define LR_ERROR_HANDLER_CONTINUE               LR_ON_ERROR_CONTINUE
#define LR_ERROR_HANDLER_SKIP_TO_NEXT_ACTION    LR_ON_ERROR_SKIP_TO_NEXT_ACTION
#define LR_ERROR_HANDLER_SKIP_TO_NEXT_ITERATION LR_ON_ERROR_SKIP_TO_NEXT_ITERATION
#define LR_ERROR_HANDLER_END_VUSER              LR_ON_ERROR_END_VUSER

/* Currently disabled
int    lr_error_context_set_entry PROTO((LPCSTR key, LPCSTR value));
*/

LPCSTR lr_error_context_get_entry PROTO((LPCSTR key));

/* Currently disabled
int    lr_error_context_remove_entry PROTO((LPCSTR key));
*/

long   lr_error_context_get_error_id PROTO((void));


/*******************************************************/
/********** Vuser new table param functions ************/
/*******************************************************/

int lr_table_get_rows_num PROTO((LPCSTR param_name));

int lr_table_get_cols_num PROTO((LPCSTR param_name));

LPCSTR lr_table_get_cell_by_col_index PROTO((LPCSTR param_name, int row, int col));

LPCSTR lr_table_get_cell_by_col_name PROTO((LPCSTR param_name, int row, const char* col_name));

int lr_table_get_column_name_by_index PROTO((LPCSTR param_name, int col, 
											LPSTR * const col_name,
											size_t * col_name_len));

int lr_table_get_column_name_by_index_free PROTO((LPSTR col_name));

/*****************************************************************************/
/******************** Vuser string compression functions *********************/
/* ***************************************************************************/
/* Compress or decompress the info in source parameter's buffer and store the 
/* result to dest parameter's buffer.
/* Parameters:
/*      param1 and param2 can be "source=<parameter name>" and
/*      "target=<parameter name>". The two parameters can be switched.
/* return:
/*      LR_PASS on success.
/*      LR_FAIL if failed.
/* ***************************************************************************/
int WINAPI lr_zip PROTO((const char* param1, const char* param2));
int WINAPI lr_unzip PROTO((const char* param1, const char* param2));

/***************************************************************************************************/
/***************************************************************************************************/
/***************************************************************************************************/
/*********************** The following functions and definitions are obsolete **********************/
/***************************************************************************************************/
/***************************************************************************************************/
/***************************************************************************************************/
/* From here on, all the functions are for internal usage and backwards compatability only         */

/********************************************/
/********************************************/
/***** Vuser parameterization functions *****/
/********************************************/
/********************************************/
/* lr_param_substit is obselete. Use lr_eval_string_ext */
int WINAPI lr_param_substit PROTO((LPCSTR file,
                                   int const line,
                                   LPCSTR in_str,
                                   size_t const in_len,
                                   LPSTR * const out_str,
                                   size_t * const out_len));
void WINAPI lr_param_substit_free PROTO((LPSTR * pstr));


/* different sensitivity values. These are used by the drivers to determine
   their sensitivity to operations from the viewer in stand alone mode, and
   from the controller. The more sensitive the driver, the more time it takes
   to run the test.
   Default values: in stand alone mode - high sensitivity,
                   when running from the controller - low sensitivity
   This can be overriden by calling the driver with the command line
   -sensitivity <number>
*/

#define LR_SENSITIVITY_LOW                                0
#define LR_SENSITIVITY_HIGH                               9


LPSTR WINAPI lrfnc_eval_string PROTO((LPCSTR str,
                                      LPCSTR file_name,
                                      long const line_num));


int WINAPI lrfnc_save_string PROTO(( const char * param_val,
                                     const char * param_name,
                                     const char * file_name,
                                     long const line_num));

int WINAPI lrfnc_free_parameter PROTO((const char * param_name ));

#define DEFAULT_TIME_STAMP_DIGITS 13 //Default number of digits in timestamp.
#define MAX_TIME_STAMP_DIGITS 16 //Max number of difits in timestamp.

#if DEFAULT_TIME_STAMP_DIGITS > MAX_TIME_STAMP_DIGITS
#error Definition of DEFAULT_TIME_STAMP_DIGITS is wrong
#endif

typedef struct _lr_timestamp_param
{
	int iDigits;
}lr_timestamp_param;

extern const lr_timestamp_param default_timestamp_param;

int WINAPI lrfnc_save_timestamp PROTO((const char * param_name, const lr_timestamp_param* time_param));

int lr_save_searched_string(char *buffer, long buf_size, unsigned int occurrence,
			    char *search_string, int offset, unsigned int param_val_len, 
			    char *param_name);

/* lr_string is obselete. Use lr_eval_string */
LPSTR WINAPI lr_string PROTO((LPCSTR str));

/* These functions are for internal use only */
#if defined(LR_UNIX) && defined(IBM)
/* For internal use with the driver on IBM */
#define LR_FUNC_NAME(a)         a

#define LR_FUNC_LIST \
			LR_FUNC_MAC(lr_init) \
			LR_FUNC_MAC(lr_main_loop) \
			LR_FUNC_MAC(lr_abort) \
			LR_FUNC_MAC(lr_start_transaction) \
			LR_FUNC_MAC(lr_end_transaction) \
			LR_FUNC_MAC(lr_start_cross_vuser_transaction) \
			LR_FUNC_MAC(lr_end_cross_vuser_transaction) \
			LR_FUNC_MAC(lr_rendezvous) \
			LR_FUNC_MAC(lr_user_data_point) \
			LR_FUNC_MAC(lr_peek_events) \
			LR_FUNC_MAC(lr_whoami) \
			LR_FUNC_MAC(lr_get_host_name) \
			LR_FUNC_MAC(lr_get_master_host_name) \
			LR_FUNC_MAC(lr_new_prefix) \
			LR_FUNC_MAC(lr_msg) \
			LR_FUNC_MAC(lr_debug_message) \
			LR_FUNC_MAC(lr_set_debug_message) \
			LR_FUNC_MAC(lr_get_debug_message) \
			LR_FUNC_MAC(lr_get_attrib_long) \
			LR_FUNC_MAC(lr_get_attrib_string) \
			LR_FUNC_MAC(lr_get_attrib_double) \
			LR_FUNC_MAC(lrfnc_eval_string) \
			LR_FUNC_MAC(lr_param_substit) \
			LR_FUNC_MAC(lr_eval_string_ext) \
			LR_FUNC_MAC(lr_eval_string_ext_free) \
			LR_FUNC_MAC(lr_param_substit_free) \
			LR_FUNC_MAC(lr_next_row) \
			LR_FUNC_MAC(lr_advance_param) \
			LR_FUNC_MAC(lr_think_time) \
			LR_FUNC_MAC(lr_force_think_time) \
			LR_FUNC_MAC(lr_log_message) \
			LR_FUNC_MAC(lr_message) \
			LR_FUNC_MAC(lr_error_message) \
			LR_FUNC_MAC(lr_output_message) \
			LR_FUNC_MAC(lr_eval_string) \
			LR_FUNC_MAC(lr_save_var) \
			LR_FUNC_MAC(lr_save_string) \
			LR_FUNC_MAC(lr_free_parameter) \
			LR_FUNC_MAC(lr_save_int) \
			LR_FUNC_MAC(lr_save_timestamp) \
			LR_FUNC_MAC(lr_save_param_regexp) \
			LR_FUNC_MAC(lr_save_datetime) \
			LR_FUNC_MAC(lr_vuser_status_message) \
			LR_FUNC_MAC(lr_string) \
			LR_FUNC_MAC(lr_printf) \
			LR_FUNC_MAC(lr_send_port) \
			LR_FUNC_MAC(lr_error_message_without_fileline) \
			LR_FUNC_MAC(lr_localtime) \
			LR_FUNC_MAC(lr_double_think_time) \
			LR_FUNC_MAC(lr_terminate) \
			LR_FUNC_MAC(lr_save_value) \
			LR_FUNC_MAC(lrfnc_save_string) \
			LR_FUNC_MAC(lrfnc_free_parameter) \
			LR_FUNC_MAC(lrfnc_save_timestamp) \
			LR_FUNC_MAC(lr_log_think_time) \
			LR_FUNC_MAC(lr_internal_think_time) \
			LR_FUNC_MAC(lr_get_calculated_think_time) \
			LR_FUNC_MAC(lr_continue_on_error) \
			LR_FUNC_MAC(lr_decrypt) \
			LR_FUNC_MAC(lr_zip) \
			LR_FUNC_MAC(lr_unzip)

#endif
int WINAPI lr_save_value PROTO((LPCSTR param_val,
                                unsigned long const param_val_len,
                                unsigned long const options,
                                LPCSTR param_name,
                                LPCSTR file_name,
                                long const line_num));


/********************************************/
/********************************************/
/********** Vuser output functions **********/
/********************************************/
/********************************************/
#define LR_MSG_CLASS_BRIEF_LOG      LR_MSG_CLASS_STANDARD_LOG
#define LR_MSG_CLASS_RESULT_DATA    LR_MSG_CLASS_RETURNED_DATA
#define LR_MSG_CLASS_FULL_TRACE     LR_MSG_CLASS_ADVANCED_TRACE
#define LR_MSG_CLASS_AUTO_LOG       LR_MSG_CLASS_JIT_LOG_ON_ERROR
#define LR_MSG_OFF                  LR_SWITCH_OFF
#define LR_MSG_ON                   LR_SWITCH_ON
#define LR_MSG_DEFAULT              LR_SWITCH_DEFAULT

#define LR_MSG_INTERNAL  8
#define LR_MSG_INTERNAL_TO_BRIDGE 16
#define LR_MSG_PORT_NUM 50 	/* 32 for port num | internal | send */
int LR_MSG_FUNC lr_printf PROTO((LPCSTR fmt, ...));
/* internal usage */
int WINAPI lr_set_debug_message PROTO((unsigned int msg_class,
                                       unsigned int swtch));
unsigned int WINAPI lr_get_debug_message PROTO((void));


/********************************************/
/********************************************/
/********* Vuser think time functions *******/
/********************************************/
/********************************************/
#define Think(a) lr_think_time(a)
void WINAPI lr_double_think_time PROTO(( double secs));
void WINAPI lr_usleep PROTO((long));


/********************************************/
/********************************************/
/********* Vuser general information ********/
/********************************************/
/********************************************/
/* On ars machine (for IBM) function locatime receives int.
   On the rest of the platform it receives long. */
#ifdef IBM
int * WINAPI lr_localtime PROTO((int offset));
#else
int * WINAPI lr_localtime PROTO((long offset));
#endif /* IBM */

int LR_MSG_FUNC lr_send_port PROTO((long port));
#define LR_PARAM void FAR *

#ifndef CCI

#ifdef LR_UNIX
void lr_init PROTO((int *argc, char **argv));
#else
#ifdef _WIN32
#define LRUN_STAMP		"XXXMercuryXLWin32b.040902"
#else
#define LRUN_STAMP		"XXXMercuryXLWin16b.040902"
#endif /* _WIN32 */

#define lr_init(hinst,command_params) \
        _lr_init(hinst, command_params, LRUN_H_VER, LRUN_STAMP);

void WINAPI _lr_init (HINSTANCE, LPSTR, long, LPCSTR);

int WINAPI lr_pp_init ();
int WINAPI lr_pt_init ();

#endif /* LR_UNIX */

/* Call lr_terminate() right after lr_main_loop in your main.c file */
void WINAPI lr_terminate PROTO((void));
int WINAPI lr_pp_terminate PROTO((void));
int WINAPI lr_pt_terminate PROTO((void));


void WINAPI lr_set_run_function PROTO((int (LR_FUNC *func)(LR_PARAM)));
void WINAPI lr_set_general_function PROTO((int (LR_FUNC *func)(LR_PARAM)));
void WINAPI lr_set_end_function PROTO((int (LR_FUNC *func)(LR_PARAM)));
void WINAPI lr_set_pause_function PROTO((int (LR_FUNC *func)(LR_PARAM)));

#ifndef LR_UNIX
/* functions for interfering in Windows message handling */
void WINAPI lr_set_pre_translate_function(int (LR_FUNC FAR *func)(MSG *));
void WINAPI lr_set_pre_dispatch_function(int (LR_FUNC FAR *func)(MSG *));

/* these declared functions should return one of the following: */
#define LR_IGNORE_MESSAGE      1
#define LR_TRANSLATE_MESSAGE   2	/* for pre-translate only  */
#define LR_DISPATCH_MESSAGE    3

#endif /* !defined(LR_UNIX) */

/* entering the main loop or aborting */

void WINAPI lr_main_loop PROTO((void));
#endif /* !defined (CCI) */


struct _lr_declare_identifier{
	char signature[24];
	char value[128];
};

int WINAPI lr_pt_abort PROTO((void));

void vuser_declaration PROTO((void));

#define BEGIN_VUSER_DECLARATION void vuser_declaration() {
#define DECLARE_VUSER_TRANSACTION(t) lr_declare_transaction(t);
#define DECLARE_VUSER_RENDEZVOUS(r) lr_declare_rendezvous(r);
#define END_VUSER_DECLARATION return;}

#if defined(LR_UNIX) &&  ! defined(__STDC__)
#define DECLARE_VUSER_RUN( n, f) \
   do {\
           int LR_FUNC f (); \
           lr_set_run_function(f); \
           {\
           static char array[][48] = {\
               "XXXVuserRunF:", \
               n \
            };\
        }\
  } while (0);
#else
#define DECLARE_VUSER_RUN( n, f) \
   do {\
           int LR_FUNC f (LR_PARAM p); \
           lr_set_run_function(f); \
           {\
           static char array[][48] = {\
               "XXXVuserRunF:", \
               n \
            };\
        }\
  } while (0);
#endif


#ifdef DOSMSWIN
void WINAPI _lr_prefix(int, LPCSTR filename, int line);
void WINAPI _lr_no_prefix(void);
int LR_MSG_FUNC _lr_output PROTO((LPCSTR fmt, ...));
#endif

#ifdef LR_UNIX
extern int (*lr_message_func)PROTO((const char *, ...));
extern int lr_always_false;
void lrmain                   PROTO((int, char **, char **));
#endif


/********************************************/
/********************************************/
/**** Vuser transaction related functions ***/
/********************************************/
/********************************************/
#define lr_declare_transaction(transaction_name) \
	{ \
		static struct _lr_declare_identifier trans = { \
			"XXXVuserTransaction:", transaction_name \
		}; \
		_lr_declare_transaction(trans.value); \
	}
int WINAPI  _lr_declare_transaction   PROTO((LPCSTR transaction_name));


/********************************************/
/********************************************/
/******** Vuser rendezvous functions ********/
/********************************************/
/********************************************/
#define lr_declare_rendezvous(rendezvous_name) \
	{ \
		static struct _lr_declare_identifier rend = { \
			"XXXVuserRendezvous:", rendezvous_name \
		}; \
		_lr_declare_rendezvous(rend.value); \
	}
int WINAPI _lr_declare_rendezvous  PROTO((LPCSTR rendezvous_name));

/*****************************************************/
/*****************************************************/
/** Vuser functions for changing RTS on the fly ******/
/*****************************************************/
/*****************************************************/

/* Enable/Disable IpSpoofing on the fly */
int lr_enable_ip_spoofing();
int lr_disable_ip_spoofing();


/* String conversion between the following encodings */
#define LR_ENC_SYSTEM_LOCALE	NULL
#define LR_ENC_UTF8				"utf-8"
#define LR_ENC_UNICODE			"ucs-2"

int lr_convert_string_encoding(char *sourceString, char *fromEncoding, char *toEncoding, char *paramName);
#ifdef __cplusplus
}
#endif


/* Add definitions of 32-bit time functions for msvcr80.dll on Windows */
/* for compiled vusers */
#ifdef CCI
#ifdef WINNT
#define ctime _ctime32
#define difftime _difftime32
#define ftime _ftime32
#define futime _futime32
#define gmtime _gmtime32
#define localtime _localtime32
#define mktime _mktime32
#define time _time32
#define utime _utime32
#define wctime _wctime32
#define wutime _wutime32
#endif
#endif /* CCI */

#endif   /* Include me once */
