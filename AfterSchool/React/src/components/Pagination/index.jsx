import React, { useEffect } from 'react';
import { makeStyles } from '@material-ui/core/styles';
import { Pagination, PaginationItem } from '@material-ui/lab';
import { useSearchParams } from 'react-router-dom';
import { Button } from '@material-ui/core';
import { startCase } from 'lodash';
import { buildQueryParams } from 'utils';

const useStyles = makeStyles(theme => ({
  root: {
    '& > *': {
      marginTop: theme.spacing(2)
    },
    '& .MuiPaginationItem-rounded': {
      borderRadius: 0
    },
    '& .MuiButtonBase-root': {
      fontSize: '14px'
    }
  },
  itemSelected: {
    backgroundColor: `${theme.palette.primary.dark} !important`,
    color: 'white'
  },
  itemButton: {
    height: 32,
    borderRadius: 0
  }
}));

const DEFAULT_PAGE = 1;
export default function PaginationRounded({ totalPages = 50 }) {
  const classes = useStyles();
  const [searchParams, setSearchParams] = useSearchParams();
  const currentPage = Number(searchParams.get('pageNo')) || 1;

  useEffect(() => {
    if (!searchParams.get('pageNo')) setSearchParams(buildQueryParams('pageNo', DEFAULT_PAGE));
  }, []);

  const handlePageChange = (_event, value) => {
    setSearchParams(buildQueryParams('pageNo', value, searchParams));
  };

  const renderItem = item => {
    if (['previous', 'next'].includes(item.type))
      return (
        <Button {...item} className={classes.itemButton}>
          {startCase(item.type)}
        </Button>
      );
    return (
      <PaginationItem
        classes={{
          selected: classes.itemSelected
        }}
        {...item}
        style={{ margin: 0 }}
      />
    );
  };

  return (
    <div className={classes.root}>
      <Pagination
        count={totalPages}
        page={currentPage}
        variant="outlined"
        onChange={handlePageChange}
        shape="rounded"
        renderItem={renderItem}
      />
    </div>
  );
}
