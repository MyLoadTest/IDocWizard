/* -------------------------------------------------------------------------------------------------------------------- */
/* Automatically created data declarations                                                                              */
/* Data declarations for IDoc segments                                                                                  */
/* of IDoc type ZISUPODMAS_BAPIZBUSMASSENDEM01                                                                          */
/* Creation 21.02.2013 10:29:43                                                                                         */
/* Client 100                                                                                                           */
/* System AUV                                                                                                           */
/* Release 701                                                                                                          */
/* Lang. EN                                                                                                             */
/* Transaction WE60                                                                                                     */
/* Output for Release 701                                                                                               */
/* Version 3 of IDoc record types : IDoc record types for SAP Release 4.0                                               */
/* -------------------------------------------------------------------------------------------------------------------- */
/* Generic IDoc record structures                                                                                       */
/* -------------------------------------------------------------------------------------------------------------------- */
#ifndef EDI_DC40
#define EDI_DC40
typedef struct edi_dc40                           /* IDoc Control Record for Interface to External System               */
    {
    Char tabnam[10];                              /* Name of Table Structure                                            */
    Char mandt[3];                                /* Client                                                             */
    Char docnum[16];                              /* IDoc number                                                        */
    Char docrel[4];                               /* SAP Release for IDoc                                               */
    Char status[2];                               /* Status of IDoc                                                     */
    Char direct[1];                               /* Direction                                                          */
    Char outmod[1];                               /* Output mode                                                        */
    Char exprss[1];                               /* Overriding in inbound processing                                   */
    Char test[1];                                 /* Test flag                                                          */
    Char idoctyp[30];                             /* Name of basic type                                                 */
    Char cimtyp[30];                              /* Extension (defined by customer)                                    */
    Char mestyp[30];                              /* Message type                                                       */
    Char mescod[3];                               /* Message code                                                       */
    Char mesfct[3];                               /* Message Function                                                   */
    Char std[1];                                  /* EDI standard, flag                                                 */
    Char stdvrs[6];                               /* EDI standard, version and release                                  */
    Char stdmes[6];                               /* EDI message type                                                   */
    Char sndpor[10];                              /* Sender port (SAP System, external subsystem)                       */
    Char sndprt[2];                               /* Partner type of sender                                             */
    Char sndpfc[2];                               /* Partner Function of Sender                                         */
    Char sndprn[10];                              /* Partner Number of Sender                                           */
    Char sndsad[21];                              /* Sender address (SADR)                                              */
    Char sndlad[70];                              /* Logical address of sender                                          */
    Char rcvpor[10];                              /* Receiver port                                                      */
    Char rcvprt[2];                               /* Partner Type of Receiver                                           */
    Char rcvpfc[2];                               /* Partner function of recipient                                      */
    Char rcvprn[10];                              /* Partner Number of Receiver                                         */
    Char rcvsad[21];                              /* Recipient address (SADR)                                           */
    Char rcvlad[70];                              /* Logical address of recipient                                       */
    Char credat[8];                               /* Created on                                                         */
    Char cretim[6];                               /* Created at                                                         */
    Char refint[14];                              /* Transmission file (EDI Interchange)                                */
    Char refgrp[14];                              /* Message group (EDI Message Group)                                  */
    Char refmes[14];                              /* Message (EDI Message)                                              */
    Char arckey[70];                              /* Key for external message archive                                   */
    Char serial[20];                              /* Serialization                                                      */
    }edi_dc40;
#endif  EDI_DC40
#ifndef EDI_DD40
#define EDI_DD40
typedef struct edi_dd40                           /* IDoc Data Record for Interface to External System                  */
    {
    Char segnam[30];                              /* Segment (external name)                                            */
    Char mandt[3];                                /* Client                                                             */
    Char docnum[16];                              /* IDoc number                                                        */
    Char segnum[6];                               /* Segment Number                                                     */
    Char psgnum[6];                               /* Number of superior parent segment                                  */
    Char hlevel[2];                               /* Hierarchy level of SAP segment                                     */
    Char sdata[1000];                             /* Application data                                                   */
    }edi_dd40;
#endif  EDI_DD40
#ifndef EDI_DS40
#define EDI_DS40
typedef struct edi_ds40                           /* IDoc Status Record for Interface to External System                */
    {
    Char tabnam[10];                              /* Name of Table Structure                                            */
    Char mandt[3];                                /* Client                                                             */
    Char docnum[16];                              /* IDoc number                                                        */
    Char logdat[8];                               /* Date of status information                                         */
    Char logtim[6];                               /* Time of status information                                         */
    Char status[2];                               /* Status of IDoc                                                     */
    Char stamqu[3];                               /* Status for message in status record                                */
    Char stamid[20];                              /* Message for status notification: Message class                     */
    Char stamno[3];                               /* Message number for status message                                  */
    Char statyp[1];                               /* ABAP message type (A, W, E, S, I) in status message                */
    Char stapa1[50];                              /* First parameter for message in status record                       */
    Char stapa2[50];                              /* Second parameter for message in status record                      */
    Char stapa3[50];                              /* Third parameter for message in status record                       */
    Char stapa4[50];                              /* Fourth parameter for message in status record                      */
    Char statxt[70];                              /* Status text                                                        */
    Char uname[12];                               /* User Name                                                          */
    Char repid[30];                               /* Program                                                            */
    Char routid[30];                              /* Subroutine (routine, function module)                              */
    Char segnum[6];                               /* Segment Number                                                     */
    Char segfld[30];                              /* Segment field                                                      */
    Char refint[14];                              /* Transmission file (EDI Interchange)                                */
    Char refgrp[14];                              /* Message group (EDI Message Group)                                  */
    Char refmes[14];                              /* Message (EDI Message)                                              */
    Char arckey[70];                              /* Key for external message archive                                   */
    }edi_ds40;
#endif  EDI_DS40
/* End of generic IDoc record structures                                                                                */
/* -------------------------------------------------------------------------------------------------------------------- */
/* Segment structures for IDoc type ZISUPODMAS_BAPIZBUSMASSENDEM01                                                      */
/* -------------------------------------------------------------------------------------------------------------------- */
#ifndef Z2BP_HEADER000
#define Z2BP_HEADER000
typedef struct z2bp_header000                     /* Header Segment Structure                                           */
    {
    Char from[10];                                /* sender of IDoc                                                     */
    Char to[10];                                  /* DESTINATION OF THE IDOC                                            */
    Char message_id[36];                          /* MESSAGE ID FOR THE IDOC                                            */
    Char message_date[28];                        /* DATE THE MESSAGE IS BEING SENT                                     */
    Char transaction_group[4];                    /* TRANSACTION GROUP                                                  */
    Char priority[10];                            /* PRIORITY                                                           */
    Char security_context[15];                    /* SECURITY CONTEXT                                                   */
    Char market[15];                              /* MARKET                                                             */
    Char subject[30];                             /* Subject                                                            */
    }z2bp_header000;
#endif  Z2BP_HEADER000
#ifndef Z2BP_TRANSACTION000
#define Z2BP_TRANSACTION000
typedef struct z2bp_transaction000                /* Transaction Segment                                                */
    {
    Char transactionid[36];                       /* Transaction ID                                                     */
    Char transactiondate[30];                     /* EDI Receipt Date Time                                              */
    Char initiatingtransactionid[36];             /* Initiating Transaction ID                                          */
    Char receiptdate[30];                         /* EDI Receipt Date Time                                              */
    Char transactiontype[4];                      /* TYPE OF TRANSACTION                                                */
    Char changereasoncode[4];                     /* Change reason code                                                 */
    Char proposedchangedate[8];                   /* Proposed date                                                      */
    Char actualchangedate[8];                     /* Actual date                                                        */
    Char actualenddate[8];                        /* Actual end date                                                    */
    Char explanation[120];                        /* Explanation                                                        */
    }z2bp_transaction000;
#endif  Z2BP_TRANSACTION000
/* End of segment structures for IDoc type ZISUPODMAS_BAPIZBUSMASSENDEM01                                               */
/* -------------------------------------------------------------------------------------------------------------------- */
