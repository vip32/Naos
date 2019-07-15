```
       |-----|-----|-----|-----|-----|-----|-----|-----|-----|-----| =100%

INBREQ [SPAN /api/customers-------------------------]                                        SERVER receives request
......     |---[SPAN validatemodel -----------------]                                        INTERNAL validates the model
INBREQ     |---[SPAN /api/accounts------------------]                                        SERVER receives request for data
DOMREP     :       |---[SPAN getaccount---------]                                            INTERNAL repository finds entity
DOMREP     |---[SPAN createentity---------------]                                            INTERNAL repository stores entity
DOMEVT     :       |---[SPAN entitycreated------]                                            CONSUMER handles event
MESSAG     :       |---[SPAN customercreated----]                                            PRODUCER publishes message
MESSAG     :               |---[SPAN customercreatedhandler--------]                         CONSUMER handles message, takes longer
QUEUNG     |---[SPAN emailnewcustomer ----------]                                            CONSUMER handles queue item
```

QUERIES:
- group by operationname where span=root (hit count / avg duration / total duration=sum of all durations)

### Remarks
each span has a parent span, when not it is the root span.
each child span cannot take more time than the root span.
child spans are drawn below the parent span.



the root span starttime till endtime represents 100% width. 
if any child span takes longer (max) than the root span that endtime will be used instead for 100% width.

diagram is not scaled for starttimes, each indent means a CHILD and does not represent the actual starttime.
diagram endtimes are somehow scaled and represented by the width (%) of the bar


**SpanViewModel**
- long:starttime (ms)
- long:endtime   (ms)
- long:duration  (ms)
- int:duration%  (% duration of root span)
- ienumerable<SpanViewModel>:children (server/client/internal/producer/consumer)

- root span = 100% (duration)
